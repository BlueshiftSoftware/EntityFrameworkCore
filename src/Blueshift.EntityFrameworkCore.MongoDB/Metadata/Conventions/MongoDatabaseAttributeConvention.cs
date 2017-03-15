using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Blueshift.EntityFrameworkCore.Annotations;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Blueshift.EntityFrameworkCore.Metadata.Conventions
{
    public class MongoDatabaseAttributeConvention : MongoDbModelAttributeConvention<MongoDatabaseAttribute>
    {
        public MongoDatabaseAttributeConvention([NotNull] DbContext dbContext)
            : base(dbContext)
        {
        }

        protected override IEnumerable<MongoDatabaseAttribute> GetAttributes([NotNull] Type dbContextType)
            => base.GetAttributes(dbContextType)
                .DefaultIfEmpty(new MongoDatabaseAttribute(GetDefaultDatabaseName(dbContextType)));

        private string GetDefaultDatabaseName(Type dbContextType)
            => MongoDbUtilities.ToCamelCase(Regex.Replace(dbContextType.Name, pattern: "(?:Mongo)?DbContext$", replacement: ""));
    }
}