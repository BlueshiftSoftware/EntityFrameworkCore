using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using MongoDB.Bson;

namespace Blueshift.EntityFrameworkCore.MongoDB.Metadata.Conventions
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class MongoDbPropertyMappingValidationConvention : PropertyMappingValidationConvention
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Type FindCandidateNavigationPropertyType(PropertyInfo propertyInfo)
            => propertyInfo.IsDefined(typeof(ComplexTypeAttribute)) ||
                (propertyInfo.PropertyType.TryGetSequenceType() ?? propertyInfo.PropertyType).GetTypeInfo().IsDefined(typeof(ComplexTypeAttribute))
                ? null
                : base.FindCandidateNavigationPropertyType(propertyInfo);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override bool IsMappedPrimitiveProperty(Type clrType)
            => clrType == typeof(ObjectId) ||
                (clrType.TryGetSequenceType() ?? clrType).GetTypeInfo().IsDefined(typeof(ComplexTypeAttribute)) ||
                base.IsMappedPrimitiveProperty(clrType);
    }
}