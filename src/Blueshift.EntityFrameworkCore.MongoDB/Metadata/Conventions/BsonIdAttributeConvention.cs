using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
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
    public class BsonIdAttributeConvention : PropertyAttributeConvention<BsonIdAttribute>
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override InternalPropertyBuilder Apply([NotNull] InternalPropertyBuilder propertyBuilder,
            [NotNull] BsonIdAttribute attribute,
            [NotNull] MemberInfo clrMember)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));
            Check.NotNull(attribute, nameof(attribute));
            Check.NotNull(clrMember, nameof(clrMember));

            var entityType = propertyBuilder.Metadata.DeclaringEntityType;
            if (entityType.BaseType != null)
            {
                return propertyBuilder;
            }

            var entityTypeBuilder = entityType.Builder;
            var properties = new List<string> { propertyBuilder.Metadata.Name };
            ConsolidateKeys(entityType.Builder, properties);
            entityTypeBuilder.PrimaryKey(properties, ConfigurationSource.DataAnnotation);
            propertyBuilder.ValueGenerated(ValueGenerated.OnAdd, ConfigurationSource.Convention);

            return propertyBuilder;
        }

        private void ConsolidateKeys(InternalEntityTypeBuilder entityTypeBuilder, List<string> keyProperties)
        {
            var currentKey = entityTypeBuilder.Metadata.FindPrimaryKey();
            if (currentKey != null
                && entityTypeBuilder.Metadata.GetPrimaryKeyConfigurationSource() == ConfigurationSource.DataAnnotation)
            {
                keyProperties.AddRange(currentKey.Properties
                    .Where(p => !keyProperties.Any(name => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)))
                    .Select(p => p.Name));
                keyProperties.Sort(StringComparer.OrdinalIgnoreCase);
                entityTypeBuilder.RemoveKey(currentKey, ConfigurationSource.DataAnnotation);
            }
        }
    }
}