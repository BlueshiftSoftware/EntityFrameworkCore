using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    public class BsonIgnoreAttributeConvention : IEntityTypeAddedConvention
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            Type clrType = entityTypeBuilder.Metadata.ClrType;
            if (clrType == null)
            {
                return entityTypeBuilder;
            }

            IEnumerable<MemberInfo> members = clrType.GetRuntimeProperties()
                .Cast<MemberInfo>()
                .Concat(clrType.GetRuntimeFields())
                .Where(memberInfo => memberInfo.IsDefined(typeof(BsonIgnoreAttribute), true));

            foreach (MemberInfo member in members)
            {
                entityTypeBuilder.Ignore(member.Name, ConfigurationSource.DataAnnotation);
            }

            return entityTypeBuilder;
        }
    }
}