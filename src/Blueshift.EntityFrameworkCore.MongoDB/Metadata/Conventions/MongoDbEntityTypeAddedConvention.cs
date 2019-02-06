using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.MongoDB.Metadata.Conventions
{
    /// <inheritdoc cref="IEntityTypeAddedConvention" />
    /// <inheritdoc cref="IBaseTypeChangedConvention" />
    public class MongoDbEntityTypeAddedConvention : IEntityTypeAddedConvention
    {
        /// <inheritdoc />
        public InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            AddBaseType(entityTypeBuilder);

            return entityTypeBuilder;
        }

        private void AddBaseType(InternalEntityTypeBuilder entityTypeBuilder)
        {
            EntityType entityType = entityTypeBuilder.Metadata;
            Type baseType = entityType.ClrType?.GetTypeInfo().BaseType;

            if (baseType != null && baseType != typeof(object))
            {
                entityType.Model.GetOrAddEntityType(baseType);
            }
        }
    }
}
