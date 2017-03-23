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
                .Where(entityType => entityType.HasClrType()
                    && entityType.ClrType.GetTypeInfo().IsDefined(typeof(BsonKnownTypesAttribute), false))
                .SelectMany(entityType => GetAllKnownTypes(entityType.ClrType))
                .Distinct()
                .Where(knownType => model.FindEntityType(knownType) == null)
                .ToList();
            foreach (var type in unregisteredKnownTypes)
            {
                modelBuilder.Entity(type, ConfigurationSource.DataAnnotation);
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