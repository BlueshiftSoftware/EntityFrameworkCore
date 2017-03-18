using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Bson.Serialization.Attributes;

namespace Blueshift.EntityFrameworkCore.Metadata.Conventions
{
    public class MongoDbRegisterKnownTypesConvention : IModelConvention
    {
        public virtual InternalModelBuilder Apply(InternalModelBuilder modelBuilder)
        {
            IModel model = Check.NotNull(modelBuilder, nameof(modelBuilder)).Metadata;
            var unregisteredKnownTypes = model
                .GetEntityTypes()
                .Where(entityType => entityType.HasClrType()
                    && entityType.ClrType.GetTypeInfo().IsDefined(typeof(BsonKnownTypesAttribute), false))
                .SelectMany(entityType => GetAllKnownTypes(entityType.ClrType))
                .Distinct()
                .Where(knownType => model.FindEntityType(knownType) == null)
                .ToList();
            foreach (var type in unregisteredKnownTypes)
            {
                modelBuilder.Entity(type, ConfigurationSource.Convention);
            }
            return modelBuilder;
        }

        private IEnumerable<Type> GetAllKnownTypes(Type type)
            => type.GetTypeInfo()
                .GetCustomAttributes<BsonKnownTypesAttribute>(false)
                .SelectMany(bsonKnownTypeAttribute => bsonKnownTypeAttribute.KnownTypes)
                .SelectMany(knownType => new[] { knownType }.Concat(GetAllKnownTypes(knownType)));
    }
}