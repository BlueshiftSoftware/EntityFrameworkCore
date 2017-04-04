using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
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
    public class MongoDbRegisterKnownTypesConvention : IModelConvention
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalModelBuilder Apply(InternalModelBuilder modelBuilder)
        {
            IModel model = Check.NotNull(modelBuilder, nameof(modelBuilder)).Metadata;
            var unregisteredKnownTypes = model
                .GetEntityTypes()
                .Where(entityType => entityType.HasClrType())
                .Select(entityType => entityType.ClrType)
                .ToList();
            foreach (var type in unregisteredKnownTypes)
            {
                RegisterKnownTypes(modelBuilder, type);
            }
            return modelBuilder;
        }

        private void RegisterKnownTypes(InternalModelBuilder modelBuilder, Type baseType)
        {
            IEnumerable<Type> knownTypes = baseType.GetTypeInfo()
                .GetCustomAttributes<BsonKnownTypesAttribute>(false)
                .SelectMany(bsonKnownTypeAttribute => bsonKnownTypeAttribute.KnownTypes)
                .ToList();
            foreach (var derivedType in knownTypes)
            {
                modelBuilder
                    .Entity(derivedType, ConfigurationSource.Convention)
                    .HasBaseType(baseType, ConfigurationSource.Convention)
                    .MongoDb()
                    .IsDerivedType = true;
                RegisterKnownTypes(modelBuilder, derivedType);
            }
        }
    }
}