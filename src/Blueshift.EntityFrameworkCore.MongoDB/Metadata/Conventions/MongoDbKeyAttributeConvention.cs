using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Bson.Serialization.Attributes;

namespace Blueshift.EntityFrameworkCore.MongoDB.Metadata.Conventions
{
    /// <inheritdoc />
    public class MongoDbKeyAttributeConvention : KeyAttributeConvention
    {
        private static readonly KeyAttribute KeyAttribute = new KeyAttribute();

        /// <inheritdoc />
        public override InternalPropertyBuilder Apply(InternalPropertyBuilder propertyBuilder)
        {
            Check.NotNull(propertyBuilder, nameof(propertyBuilder));

            MemberInfo memberInfo = propertyBuilder.Metadata.GetIdentifyingMemberInfo();
            return (memberInfo?.IsDefined(typeof(BsonIdAttribute), true) ?? false)
                ? base.Apply(propertyBuilder, KeyAttribute, memberInfo)
                : base.Apply(propertyBuilder);
        }

        /// <inheritdoc />
        public override InternalModelBuilder Apply(InternalModelBuilder modelBuilder)
        {
            IEnumerable<EntityType> entityTypes = modelBuilder.Metadata
                .GetEntityTypes()
                .Where(entityType => entityType.BaseType != null);
            foreach (EntityType entityType in entityTypes)
            {
                foreach (Property declaredProperty in entityType.GetDeclaredProperties())
                {
                    if (declaredProperty.GetIdentifyingMemberInfo()?.IsDefined(typeof(BsonIdAttribute), true) ?? false)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.KeyAttributeOnDerivedEntity(entityType.DisplayName(), declaredProperty.Name));
                    }
                }
            }
            return base.Apply(modelBuilder);
        }
    }
}