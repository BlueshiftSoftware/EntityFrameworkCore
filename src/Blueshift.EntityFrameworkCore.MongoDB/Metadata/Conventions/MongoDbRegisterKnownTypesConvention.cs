using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Bson.Serialization.Attributes;

namespace Blueshift.EntityFrameworkCore.MongoDB.Metadata.Conventions
{
    /// <inheritdoc />
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class MongoDbRegisterKnownTypesConvention : IEntityTypeAddedConvention
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder)
        {
            EntityType baseEntityType = Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder)).Metadata;
            IEnumerable<Type> knownTypes = baseEntityType.ClrType
                ?.GetTypeInfo()
                .GetCustomAttributes<BsonKnownTypesAttribute>(false)
                .SelectMany(bsonKnownTypeAttribute => bsonKnownTypeAttribute.KnownTypes)
                .ToList();

            MongoDbEntityTypeAnnotations annotations = entityTypeBuilder.MongoDb();
            if (!annotations.DiscriminatorIsRequired)
            {
                annotations.DiscriminatorIsRequired = baseEntityType.IsAbstract();
            }

            if (knownTypes != null)
            {
                InternalModelBuilder modelBuilder = entityTypeBuilder.ModelBuilder;
                foreach (Type derivedType in knownTypes)
                {
                    modelBuilder
                        .Entity(derivedType, ConfigurationSource.DataAnnotation)
                        .HasBaseType(baseEntityType, ConfigurationSource.DataAnnotation)
                        .MongoDb()
                        .IsDerivedType = true;
                }
            }
            return entityTypeBuilder;
        }
    }
}