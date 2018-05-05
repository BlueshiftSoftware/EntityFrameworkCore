using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.MongoDB.Metadata.Conventions
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ComplexTypePropertyDiscoveryConvention : IEntityTypeAddedConvention, IBaseTypeChangedConvention
    {
        private readonly ITypeMappingSource _typeMappingSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ComplexTypePropertyDiscoveryConvention([NotNull] ITypeMappingSource typeMappingSource)
        {
            Check.NotNull(typeMappingSource, nameof(typeMappingSource));

            _typeMappingSource = typeMappingSource;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            var entityType = entityTypeBuilder.Metadata;

            if (entityType.HasClrType())
            {
                var complexTypeProperties = entityType.ClrType.GetRuntimeProperties().Where(IsCandidateComplexTypeProperty);

                foreach (var propertyInfo in complexTypeProperties)
                {
                    entityTypeBuilder.Owns(propertyInfo.PropertyType, propertyInfo, ConfigurationSource.Convention);
                }
            }

            return entityTypeBuilder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual bool IsCandidateComplexTypeProperty([NotNull] PropertyInfo propertyInfo)
            => Check.NotNull(propertyInfo, nameof(propertyInfo)).IsCandidateProperty()
                && _typeMappingSource.FindMapping(propertyInfo) == null
                && propertyInfo.PropertyType.IsClass;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool Apply(InternalEntityTypeBuilder entityTypeBuilder, EntityType oldBaseType)
            => Apply(entityTypeBuilder) != null;
    }
}
