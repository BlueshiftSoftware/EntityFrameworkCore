using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

namespace Blueshift.EntityFrameworkCore.MongoDB.Adapter.Serialization
{
    /// <summary>
    /// A serializer for writing navigation properties used by MongoDB.
    /// </summary>
    public class NavigationBsonSerializer<TClass> : BsonClassMapSerializer<TClass>
    {
        private readonly BsonClassMap _classMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationBsonSerializer{TClass}"/> class.
        /// </summary>
        /// <param name="denormalizedMemberMaps">Optional. An <see cref="IEnumerable{T}"/> of <see cref="BsonMemberMap"/> referencing the properties
        /// to be denormalized by this serializer.</param>
        public NavigationBsonSerializer(
            [CanBeNull] IEnumerable<BsonMemberMap> denormalizedMemberMaps = null)
            : base(BsonClassMap.LookupClassMap(typeof(TClass)))
        {
            _classMap = BsonClassMap.LookupClassMap(typeof(TClass));
            DenormalizedMemberMaps = (denormalizedMemberMaps ?? new BsonMemberMap[0])
                .Except(new[] { _classMap.IdMemberMap })
                .OrderBy(bsonMemberMap => bsonMemberMap.MemberName)
                .ToList();
        }

        /// <summary>
        /// Gets the <see cref="IEnumerable{T}"/> of <see cref="BsonMemberMap"/> that this serializer is configured to denormalize.
        /// </summary>
        public IEnumerable<BsonMemberMap> DenormalizedMemberMaps { get; }

        /// <inheritdoc />
        public override TClass Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            TClass item = base.Deserialize(context, args);
            return item;
        }

        /// <inheritdoc />
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TClass value)
        {
            IBsonWriter bsonWriter = context.Writer;

            Type actualType = value?.GetType();
            if (actualType == null)
            {
                bsonWriter.WriteNull();
            }
            else if (typeof(TClass).IsAssignableFrom(actualType))
            {
                SerializeClass(context, args, value);
            }
            else
            {
                throw new BsonSerializationException(
                    $"Expected an object derived from {typeof(TClass).FullName}, but received a value of type {actualType.FullName} instead.");
            }
        }

        private void SerializeClass(BsonSerializationContext context, BsonSerializationArgs args, TClass document)
        {
            IBsonWriter bsonWriter = context.Writer;

            bsonWriter.WriteStartDocument();

            SerializeMember(context, document, _classMap.IdMemberMap);

            if (ShouldSerializeDiscriminator(args.NominalType))
            {
                SerializeDiscriminator(context, args.NominalType, document);
            }

            foreach (BsonMemberMap memberMap in DenormalizedMemberMaps)
            {
                SerializeMember(context, document, memberMap);
            }

            bsonWriter.WriteEndDocument();
        }

        private void SerializeMember(BsonSerializationContext context, TClass document, BsonMemberMap memberMap)
        {
            IBsonWriter bsonWriter = context.Writer;
            object value = memberMap.Getter(document);

            if (memberMap.ShouldSerialize(document, value))
            {
                bsonWriter.WriteName(memberMap.ElementName);
                memberMap.GetSerializer().Serialize(context, value);
            }
        }

        private bool ShouldSerializeDiscriminator(Type nominalType)
            => (nominalType != _classMap.ClassType || _classMap.DiscriminatorIsRequired || _classMap.HasRootClass) && !_classMap.IsAnonymous;

        private void SerializeDiscriminator(BsonSerializationContext context, Type nominalType, object obj)
        {
            IDiscriminatorConvention discriminatorConvention = BsonSerializer.LookupDiscriminatorConvention(_classMap.ClassType);
            BsonValue discriminator = discriminatorConvention?.GetDiscriminator(nominalType, obj.GetType());
            if (discriminator != null)
            {
                context.Writer.WriteName(discriminatorConvention.ElementName);
                BsonValueSerializer.Instance.Serialize(context, discriminator);
            }
        }
    }
}
