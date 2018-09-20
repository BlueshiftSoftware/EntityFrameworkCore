using System.Reflection;
using System.Text.RegularExpressions;
using Blueshift.EntityFrameworkCore.MongoDB.Annotations;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.MongoDB.Metadata.Conventions
{
    /// <inheritdoc />
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class MongoDatabaseConvention : IModelInitializedConvention
    {
        private readonly DbContext _dbContext;
        private readonly MongoDbOptionsExtension _mongoDbOptionsExtension;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public MongoDatabaseConvention([NotNull] DbContext dbContext)
        {
            _dbContext = Check.NotNull(dbContext, nameof(dbContext));
            _mongoDbOptionsExtension = _dbContext
                .GetService<IDbContextServices>()
                .ContextOptions
                .FindExtension<MongoDbOptionsExtension>();
        }

        /// <inheritdoc />
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public InternalModelBuilder Apply(InternalModelBuilder modelBuilder)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));

            MongoDatabaseAttribute mongoDatabaseAttribute = _dbContext.GetType()
                .GetTypeInfo()
                .GetCustomAttribute<MongoDatabaseAttribute>();

            string databaseName = _mongoDbOptionsExtension.DatabaseName
                                  ?? mongoDatabaseAttribute?.Database
                                  ?? GetDefaultDatabaseName();

            Check.NotNull(modelBuilder, nameof(modelBuilder))
                .MongoDb()
                .Database = databaseName;
            return modelBuilder;
        }

        private string GetDefaultDatabaseName()
            => MongoDbUtilities.ToLowerCamelCase(Regex.Replace(_dbContext.GetType().Name, "(?:Mongo)?(?:Db)?(?:Context)?$", ""));
    }
}