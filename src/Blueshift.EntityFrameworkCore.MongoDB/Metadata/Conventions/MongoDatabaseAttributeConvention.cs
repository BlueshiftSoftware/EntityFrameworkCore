using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Blueshift.EntityFrameworkCore.MongoDB.Annotations;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Blueshift.EntityFrameworkCore.MongoDB.Metadata.Conventions
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class MongoDatabaseAttributeConvention : MongoDbModelAttributeConvention<MongoDatabaseAttribute>
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public MongoDatabaseAttributeConvention([NotNull] DbContext dbContext)
            : base(dbContext)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override InternalModelBuilder Apply(InternalModelBuilder modelBuilder)
        {
            var extension = DbContext
                .GetService<IDbContextServices>()
                ?.ContextOptions
                ?.FindExtension<MongoDbOptionsExtension>();
            if (!string.IsNullOrEmpty(extension?.DatabaseName))
            {
                modelBuilder.MongoDb().Database = extension.DatabaseName;
            }
            else
            {
                base.Apply(modelBuilder);
            }
            return modelBuilder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override IEnumerable<MongoDatabaseAttribute> GetAttributes(Type dbContextType)
            => base.GetAttributes(dbContextType)
                .DefaultIfEmpty(new MongoDatabaseAttribute(GetDefaultDatabaseName(dbContextType)));

        private string GetDefaultDatabaseName(Type dbContextType)
            => MongoDbUtilities.ToLowerCamelCase(Regex.Replace(dbContextType.Name, pattern: "(?:Mongo)?DbContext$", replacement: ""));
    }
}