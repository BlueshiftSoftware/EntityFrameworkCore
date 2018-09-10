using System;
using System.Collections.Generic;
using System.Reflection;
using Blueshift.EntityFrameworkCore.MongoDB.Adapter.Serialization;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Bson.Serialization.Serializers;

// ReSharper disable once CheckNamespace
namespace MongoDB.Bson.Serialization
{
    /// <summary>
    /// Provides extended functionality to <see cref="IBsonSerializer"/>.
    /// </summary>
    public static class BsonSerializerExtensions
    {
        /// <summary>
        /// Modifies an instance of <see cref="IBsonSerializer"/> to only use the supplied members when serializing instances.
        /// </summary>
        /// <param name="bsonSerializer">The <see cref="IBsonSerializer"/> to modify.</param>
        /// <param name="denormalizedMemberNames">An <see cref="IEnumerable{T}"/> of <see cref="string"/> that lists the members
        /// required for serialization.</param>
        /// <returns>A new instance of <see cref="IBsonSerializer"/> that serializes the information in <paramref name="denormalizedMemberNames"/>.</returns>
        public static IBsonSerializer AsDenormalizingBsonClassMapSerializer(
            [NotNull] this IBsonSerializer bsonSerializer,
            [CanBeNull] IEnumerable<string> denormalizedMemberNames = null)
        {
            TypeInfo typeInfo = Check.NotNull(bsonSerializer, nameof(bsonSerializer)).GetType().GetTypeInfo();

            if (bsonSerializer is IChildSerializerConfigurable childSerializerConfigurable)
            {
                bsonSerializer = childSerializerConfigurable.WithChildSerializer(
                    childSerializerConfigurable.ChildSerializer.AsDenormalizingBsonClassMapSerializer(denormalizedMemberNames));
            }
            else if (typeInfo.TryGetImplementationType(typeof(ReadOnlyCollectionSerializer<>),
                         out Type readOnlyCollectionSerializerType)
                     || typeInfo.TryGetImplementationType(typeof(ReadOnlyCollectionSubclassSerializer<,>),
                         out readOnlyCollectionSerializerType))
            {
                bsonSerializer = (IBsonSerializer) Activator.CreateInstance(readOnlyCollectionSerializerType,
                    ((IBsonSerializer) readOnlyCollectionSerializerType
                        .GetProperty(nameof(EnumerableSerializerBase<object[]>.ItemSerializer))
                        .GetValue(bsonSerializer))
                    .AsDenormalizingBsonClassMapSerializer(denormalizedMemberNames));
            }
            else
            {
                BsonClassMap bsonClassMap = BsonClassMap.LookupClassMap(bsonSerializer.ValueType);
                bsonSerializer = (IBsonSerializer) Activator.CreateInstance(
                    typeof(DenormalizingBsonClassMapSerializer<>).MakeGenericType(bsonSerializer.ValueType),
                    bsonClassMap,
                    denormalizedMemberNames);
            }

            return bsonSerializer;
        }
    }
}
