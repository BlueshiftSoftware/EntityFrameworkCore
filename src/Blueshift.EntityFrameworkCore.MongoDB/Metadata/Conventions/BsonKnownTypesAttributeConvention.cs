using System;
using System.Reflection;
using Blueshift.EntityFrameworkCore.MongoDB.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Bson.Serialization.Attributes;

namespace Blueshift.EntityFrameworkCore.MongoDB.Metadata.Conventions
{
    /// <inheritdoc />
    public class BsonKnownTypesAttributeConvention : EntityTypeAttributeConvention<BsonKnownTypesAttribute>
    {
        /// <inheritdoc />
        public override InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            var type = entityTypeBuilder.Metadata.ClrType;
            if (type == null || !Attribute.IsDefined(type, typeof(BsonKnownTypesAttribute), inherit: false))
            {
                return entityTypeBuilder;
            }

            var attributes = type.GetTypeInfo().GetCustomAttributes<BsonKnownTypesAttribute>(false);

            foreach (var attribute in attributes)
            {
                entityTypeBuilder = Apply(entityTypeBuilder, attribute);
                if (entityTypeBuilder == null)
                {
                    break;
                }
            }

            return entityTypeBuilder;
        }

        /// <inheritdoc />
        public override InternalEntityTypeBuilder Apply(
            InternalEntityTypeBuilder entityTypeBuilder,
            BsonKnownTypesAttribute bsonKnownTypesAttribute)
        {
            MongoDbEntityTypeAnnotations annotations = entityTypeBuilder.MongoDb();
            if (!annotations.DiscriminatorIsRequired)
            {
                annotations.DiscriminatorIsRequired = entityTypeBuilder.Metadata.IsAbstract();
            }

            if (bsonKnownTypesAttribute.KnownTypes != null)
            {
                InternalModelBuilder modelBuilder = entityTypeBuilder.ModelBuilder;
                Type baseType = entityTypeBuilder.Metadata.ClrType;

                foreach (Type derivedType in bsonKnownTypesAttribute.KnownTypes)
                {
                    if (!baseType.IsAssignableFrom(derivedType))
                    {
                        throw new InvalidOperationException($"Known type {derivedType} declared on base type {baseType} does not inherit from base type.");
                    }

                    modelBuilder
                        .Entity(derivedType, ConfigurationSource.DataAnnotation)
                        .MongoDb()
                        .IsDerivedType = true;
                }
            }

            return entityTypeBuilder;
        }
    }
}
