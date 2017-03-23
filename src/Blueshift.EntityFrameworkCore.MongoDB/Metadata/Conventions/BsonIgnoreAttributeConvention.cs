using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Bson.Serialization.Attributes;

namespace Blueshift.EntityFrameworkCore.MongoDB.Metadata.Conventions
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class BsonIgnoreAttributeConvention : PropertyAttributeConvention<BsonIgnoreAttribute>
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override InternalPropertyBuilder Apply([NotNull] InternalPropertyBuilder propertyBuilder,
            [NotNull] BsonIgnoreAttribute attribute,
            [NotNull] MemberInfo clrMember)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NotNull(attribute, nameof(attribute));
            Check.NotNull(clrMember, nameof(clrMember));

            var entityTypeBuilder = propertyBuilder.Metadata.DeclaringEntityType.Builder;
            entityTypeBuilder.Ignore(propertyBuilder.Metadata.Name, ConfigurationSource.Convention);

            return propertyBuilder;
        }
    }
}