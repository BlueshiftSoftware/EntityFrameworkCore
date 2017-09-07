using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.MongoDB.Metadata.Conventions
{
    /// <inheritdoc />
    public class MongoDbBaseTypeDiscoveryConvention : BaseTypeDiscoveryConvention
    {
        /// <inheritdoc />
        protected override EntityType FindClosestBaseType(EntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            Type baseType = entityType.ClrType?.GetTypeInfo().BaseType;
            return (baseType != null && baseType != typeof(object))
                ? entityType.Model.GetOrAddEntityType(baseType)
                : null;
        }
    }
}
