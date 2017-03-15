using System;
using Blueshift.EntityFrameworkCore.Metadata.Internal;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Bson.Serialization;

namespace Blueshift.EntityFrameworkCore.Annotations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class DerivedTypeAttribute : Attribute, IEntityTypeAttribute, IBsonClassMapAttribute
    {
        public DerivedTypeAttribute([NotNull] Type derivedType)
        {
            DerivedType = Check.NotNull(derivedType, nameof(derivedType));
        }

        public virtual Type DerivedType { get; }

        public virtual void Apply([NotNull] InternalEntityTypeBuilder entityTypeBuilder)
        {
            if (entityTypeBuilder == null)
            {
                throw new ArgumentNullException(nameof(entityTypeBuilder));
            }
            entityTypeBuilder
                .ModelBuilder
                .Entity(DerivedType, ConfigurationSource.DataAnnotation)
                .HasBaseType(entityTypeBuilder.Metadata, ConfigurationSource.DataAnnotation)
                .MongoDb(ConfigurationSource.DataAnnotation)
                .HasDiscriminator(DerivedType.Name);
        }

        public virtual void Apply([NotNull] BsonClassMap classMap)
        {
            if (classMap == null)
            {
                throw new ArgumentNullException(nameof(classMap));
            }
            classMap.AddKnownType(DerivedType);
        }
    }
}