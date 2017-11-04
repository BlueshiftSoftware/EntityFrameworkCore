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
        /// <param name="denormalizedMembers">An <see cref="IEnumerable{T}"/> of <see cref="BsonMemberMap"/> that lists the members
        /// required for serialization.</param>
        /// <returns>A new instance of <see cref="IBsonSerializer"/> that serializes the information in <paramref name="denormalizedMembers"/>.</returns>
        public static IBsonSerializer AsNavigationBsonSerializer(
            [NotNull] this IBsonSerializer bsonSerializer,
            [CanBeNull] IEnumerable<BsonMemberMap> denormalizedMembers = null)
        {
            TypeInfo typeInfo = Check.NotNull(bsonSerializer, nameof(bsonSerializer)).GetType().GetTypeInfo();

            var childSerializerConfigurable = bsonSerializer as IChildSerializerConfigurable;
            if (childSerializerConfigurable != null)
            {
                bsonSerializer = childSerializerConfigurable.WithChildSerializer(
                    childSerializerConfigurable.ChildSerializer.AsNavigationBsonSerializer(denormalizedMembers));
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
                    .AsNavigationBsonSerializer(denormalizedMembers));
            }
            else
            {
                bsonSerializer = (IBsonSerializer)Activator.CreateInstance(
                    typeof(NavigationBsonSerializer<>).MakeGenericType(bsonSerializer.ValueType),
                    denormalizedMembers);
            }

            return bsonSerializer;
        }
    }
}
