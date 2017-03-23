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
    public class MongoDbPropertyDiscoveryConvention : PropertyDiscoveryConvention
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override bool IsCandidatePrimitiveProperty(PropertyInfo propertyInfo)
            => propertyInfo.PropertyType == typeof(ObjectId) ||
                (propertyInfo.PropertyType.TryGetSequenceType() ?? propertyInfo.PropertyType).GetTypeInfo().IsDefined(typeof(ComplexTypeAttribute)) ||
                base.IsCandidatePrimitiveProperty(propertyInfo);
    }
}