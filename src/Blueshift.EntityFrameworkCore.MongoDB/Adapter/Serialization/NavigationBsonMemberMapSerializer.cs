using System;
using System.Collections.Generic;
using System.Reflection;
using Blueshift.EntityFrameworkCore.MongoDB.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Bson.Serialization;

namespace Blueshift.EntityFrameworkCore.MongoDB.Adapter.Serialization
{
    /// <inheritdoc />
    /// <summary>
    /// A serializer for writing navigation properties used by MongoDB.
    /// </summary>
    /// <typeparam name="TClass">The type of the member to be serialized by this <see cref="NavigationBsonMemberMapSerializer{TClass}"/>.</typeparam>
    public class NavigationBsonMemberMapSerializer<TClass> : IBsonSerializer<TClass>
    {
        private readonly Lazy<IBsonSerializer> _lazyBsonSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationBsonMemberMapSerializer{TClass}"/> class.
        /// </summary>
        /// <param name="bsonMemberMap">The <see cref="BsonMemberMap"/> representing the member to serialize.</param>
        public NavigationBsonMemberMapSerializer(BsonMemberMap bsonMemberMap)
        {
            MemberMap = Check.NotNull(bsonMemberMap, nameof(bsonMemberMap));

            _lazyBsonSerializer = new Lazy<IBsonSerializer>(() =>
            {
                IEnumerable<string> denormalizedMemberNames
                    = bsonMemberMap.MemberInfo
                          .GetCustomAttribute<DenormalizeAttribute>()
                          ?.MemberNames
                      ?? new string[0];
                return BsonSerializer.LookupSerializer(bsonMemberMap.MemberType)
                    .AsDenormalizingBsonClassMapSerializer(denormalizedMemberNames);
            });
        }

        /// <summary>
        /// The <see cref="BsonMemberMap"/> representing the member to serialize.
        /// </summary>
        public BsonMemberMap MemberMap { get; }

        /// <inheritdoc />
        TClass IBsonSerializer<TClass>.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
            => (TClass) Deserialize(context, args);

        /// <inheritdoc />
        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TClass value)
            => _lazyBsonSerializer.Value.Serialize(context, args, value);

        /// <inheritdoc />
        public object Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
            => _lazyBsonSerializer.Value.Deserialize(context, args);

        /// <inheritdoc />
        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
            => _lazyBsonSerializer.Value.Serialize(context, args, value);

        /// <inheritdoc />
        public Type ValueType => MemberMap.MemberType;
    }
}
