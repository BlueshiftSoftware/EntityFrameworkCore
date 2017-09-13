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
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class MongoDatabaseConvention : MongoDbModelBuiltAttributeConvention<MongoDatabaseAttribute>,
        IModelInitializedConvention
    {
        private readonly MongoDbOptionsExtension _mongoDbOptionsExtension;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public MongoDatabaseConvention([NotNull] DbContext dbContext)
            : base(dbContext)
        {
            _mongoDbOptionsExtension = DbContext
                .GetService<IDbContextServices>()
                .ContextOptions
                .FindExtension<MongoDbOptionsExtension>();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        InternalModelBuilder IModelInitializedConvention.Apply(InternalModelBuilder modelBuilder)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder))
                .MongoDb()
                .Database = _mongoDbOptionsExtension.DatabaseName ?? GetDefaultDatabaseName();
            return modelBuilder;
        }

        /// <inheritdoc />
        protected override bool Apply(InternalModelBuilder modelBuilder, MongoDatabaseAttribute mongoDatabaseAttribute)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotNull(mongoDatabaseAttribute, nameof(mongoDatabaseAttribute));
            modelBuilder.MongoDb().Database = mongoDatabaseAttribute.Database;
            return true;
        }

        private string GetDefaultDatabaseName()
            => MongoDbUtilities.ToLowerCamelCase(Regex.Replace(DbContext.GetType().Name, "(?:Mongo)?(?:Db)?Context$", ""));
    }
}