using System;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Blueshift.EntityFrameworkCore.MongoDB.Storage
{
    /// <summary>
    /// Determines whether a .NET type can be mapped to a MongoDB database type.
    /// </summary>
    public class MongoDbTypeMappingSource : TypeMappingSource, IMongoDbTypeMappingSource
    {
        private readonly ConcurrentDictionary<Type, CoreTypeMapping> _typeCache = new ConcurrentDictionary<Type, CoreTypeMapping>
        {
            [typeof(string)] = new PassThruTypeMapping(typeof(string)),
            [typeof(ObjectId)] = new PassThruTypeMapping(typeof(ObjectId))
        };

        /// <inheritdoc />
        public MongoDbTypeMappingSource([NotNull] TypeMappingSourceDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <inheritdoc />
        protected override CoreTypeMapping FindMapping(in TypeMappingInfo mappingInfo)
            => _typeCache.GetOrAdd(
                Check.NotNull(mappingInfo, nameof(mappingInfo)).ClrType,
                clrType =>
                {
                    TypeInfo typeInfo = (clrType.TryGetSequenceType() ?? clrType).UnwrapNullableType().GetTypeInfo();

                    return typeInfo.IsPrimitive
                            || typeInfo.IsValueType
                            || (typeInfo.IsClass && !typeInfo.GetProperties()
                                .Any(propertyInfo => propertyInfo.IsDefined(typeof(KeyAttribute))
                                    || propertyInfo.IsDefined(typeof(BsonIdAttribute))))
                        ? new PassThruTypeMapping(clrType)
                        : null;
                });

        private class PassThruTypeMapping : CoreTypeMapping
        {
            private PassThruTypeMapping(CoreTypeMappingParameters parameters)
                : base(parameters)
            {
            }

            public PassThruTypeMapping([NotNull] Type clrType)
                : base(new CoreTypeMappingParameters(clrType))
            {
            }

            public override CoreTypeMapping Clone(ValueConverter converter)
                => new PassThruTypeMapping(Parameters.WithComposedConverter(converter));
        }
    }
}
