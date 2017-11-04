using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.MongoDB.Metadata.Conventions
{
    /// <inheritdoc />
    public class MongoDbBaseTypeDiscoveryConvention : IEntityTypeAddedConvention
    {
        /// <inheritdoc />
        public InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            EntityType entityType = entityTypeBuilder.Metadata;
            Type baseType = entityType.ClrType?.GetTypeInfo().BaseType;
            if (baseType != null && baseType != typeof(object))
            {
                entityType.Model.GetOrAddEntityType(baseType);
            }
            return entityTypeBuilder;
        }
    }
}
