using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Blueshift.EntityFrameworkCore.MongoDB.Adapter.Serialization;
using Blueshift.EntityFrameworkCore.MongoDB.SampleDomain;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using Xunit;

namespace Blueshift.EntityFrameworkCore.MongoDB.Tests.Adapter.Serialization
{
    public class BsonSerializerExtensionsTests : IClassFixture<ZooEntityFixture>
    {
        private readonly ZooEntities _zooEntities;

        public BsonSerializerExtensionsTests(ZooEntityFixture zooEntityFixture)
        {
            _zooEntities = zooEntityFixture.Entities;
        }

        private static readonly IDiscriminatorConvention DiscriminatorConvention =
            BsonSerializer.LookupDiscriminatorConvention(typeof(Animal));

        private static string GetDiscriminator(Animal animal)
            => $"\"{DiscriminatorConvention.ElementName}\" : {DiscriminatorConvention.GetDiscriminator(typeof(Animal), animal.GetType()).ToJson()}";

        private static readonly string[] DenormalizedMemberNames =
        {
            nameof(Animal.Age),
            nameof(Animal.Height),
            nameof(Animal.Weight)
        };

        private static void VerifyChildSerialier(IBsonSerializer bsonSerializer)
        {
            var childSerializerConfigurable = bsonSerializer as IChildSerializerConfigurable;
            while (childSerializerConfigurable?.ChildSerializer is IChildSerializerConfigurable)
            {
                childSerializerConfigurable = (IChildSerializerConfigurable)childSerializerConfigurable.ChildSerializer;
            }
            var navigationBsonDocumentSerializer =
                childSerializerConfigurable?.ChildSerializer as DenormalizingBsonClassMapSerializer<Animal>;
            Assert.NotNull(navigationBsonDocumentSerializer);
            IEnumerable<string> denormalizedMemberNames = navigationBsonDocumentSerializer.DenormalizedMemberMaps
                .Select(bsonMemberMap => bsonMemberMap.MemberName)
                .OrderBy(memberName => memberName)
                .ToList();
            Assert.Equal(DenormalizedMemberNames, denormalizedMemberNames);
        }

        [Theory]
        [InlineData(typeof(Queue<Animal>))]
        [InlineData(typeof(Stack<Animal>))]
        [InlineData(typeof(SortedSet<Animal>))]
        [InlineData(typeof(List<Animal>))]
        [InlineData(typeof(HashSet<Animal>))]
        public void Can_denormalize_enumerables(Type enumerableType)
        {
            IBsonSerializer defaultSerializer = BsonSerializer.LookupSerializer(enumerableType);
            IBsonSerializer bsonSerializer = defaultSerializer
                .AsDenormalizingBsonClassMapSerializer(DenormalizedMemberNames);
            Assert.NotNull(bsonSerializer);
            Assert.IsType(defaultSerializer.GetType(), bsonSerializer);
            VerifyChildSerialier(bsonSerializer);

            var originalEnumerable = (IEnumerable<Animal>)(enumerableType == typeof(SortedSet<Animal>)
                ? Activator.CreateInstance(enumerableType, _zooEntities.Animals, new AnimalComparer())
                : Activator.CreateInstance(enumerableType, _zooEntities.Animals));
            if (enumerableType == typeof(Stack<Animal>))
            {
                originalEnumerable = originalEnumerable.Reverse();
            }
            IEnumerable<string> documents = originalEnumerable
                .Select(animal => $"{{ \"_id\" : ObjectId(\"{animal.AnimalId}\"), {GetDiscriminator(animal)}, \"Age\" : \"{animal.Age}\", \"Height\" : \"{animal.Height}\", \"Weight\" : \"{animal.Weight}\" }}");
            string expectedDocument =
                $"[{string.Join(", ", documents)}]";

            var testEnumerable = (IEnumerable<Animal>)(enumerableType == typeof(SortedSet<Animal>)
                ? Activator.CreateInstance(enumerableType, _zooEntities.Animals, new AnimalComparer())
                : Activator.CreateInstance(enumerableType, _zooEntities.Animals));

            Assert.Equal(expectedDocument, testEnumerable.ToJson(enumerableType, serializer: bsonSerializer));
        }

        [Theory]
        [InlineData(typeof(IEnumerable<Animal>), typeof(List<Animal>))]
        [InlineData(typeof(ICollection<Animal>), typeof(List<Animal>))]
        [InlineData(typeof(IList<Animal>), typeof(List<Animal>))]
        [InlineData(typeof(ISet<Animal>), typeof(HashSet<Animal>))]
        public void Can_denormalize_enumerable_interfaces(Type enumerableInterface, Type concreteType)
        {
            IBsonSerializer defaultSerializer = BsonSerializer.LookupSerializer(enumerableInterface);
            IBsonSerializer bsonSerializer = defaultSerializer
                .AsDenormalizingBsonClassMapSerializer(DenormalizedMemberNames);
            Assert.NotNull(bsonSerializer);
            Assert.IsType(defaultSerializer.GetType(), bsonSerializer);
            VerifyChildSerialier(bsonSerializer);

            var enumerable = (IEnumerable<Animal>)Activator.CreateInstance(concreteType, _zooEntities.Animals);
            IEnumerable<string> documents = enumerable
                .Select(animal => $"{{ \"_id\" : ObjectId(\"{animal.AnimalId}\"), {GetDiscriminator(animal)}, \"Age\" : \"{animal.Age}\", \"Height\" : \"{animal.Height}\", \"Weight\" : \"{animal.Weight}\" }}");
            string expectedDocument =
                $"[{string.Join(", ", documents)}]";

            Assert.Equal(expectedDocument, enumerable.ToJson(enumerableInterface, serializer: bsonSerializer));
        }

        [Theory]
        [InlineData(typeof(Dictionary<string, Animal>))]
        [InlineData(typeof(IDictionary<string, Animal>))]
        public void Can_denormalize_dictionaries(Type type)
        {
            IBsonSerializer defaultSerializer = BsonSerializer.LookupSerializer(type);
            IBsonSerializer bsonSerializer = defaultSerializer
                .AsDenormalizingBsonClassMapSerializer(DenormalizedMemberNames);
            Assert.NotNull(bsonSerializer);
            Assert.IsType(defaultSerializer.GetType(), bsonSerializer);
            VerifyChildSerialier(bsonSerializer);

            Dictionary<string, Animal> dictionary = _zooEntities.Animals.ToDictionary(animal => animal.Name);
            IEnumerable<string> documents = dictionary
                .OrderBy(kvp => kvp.Key)
                .ThenBy(kvp => kvp.Value.Height)
                .Select(kvp => kvp.Value)
                .Select(animal => $"\"{animal.Name}\" : {{ \"_id\" : ObjectId(\"{animal.AnimalId}\"), {GetDiscriminator(animal)}, \"Age\" : \"{animal.Age}\", \"Height\" : \"{animal.Height}\", \"Weight\" : \"{animal.Weight}\" }}");
            string expectedDocument =
                $"{{ {string.Join(", ", documents)} }}";

            Assert.Equal(expectedDocument, dictionary.ToJson(type, serializer: bsonSerializer));
        }

        [Theory]
        [InlineData(typeof(Animal[]))]
        [InlineData(typeof(Animal[,]))]
        [InlineData(typeof(Animal[,,]))]
        public void Can_denormalize_array(Type type)
        {
            IBsonSerializer defaultSerializer = BsonSerializer.LookupSerializer(type);
            IBsonSerializer bsonSerializer = defaultSerializer
                .AsDenormalizingBsonClassMapSerializer(DenormalizedMemberNames);
            Assert.NotNull(bsonSerializer);
            Assert.IsType(defaultSerializer.GetType(), bsonSerializer);
            VerifyChildSerialier(bsonSerializer);
        }

        [Fact]
        public void Can_denormalize_ReadOnlyCollection()
        {
            IBsonSerializer defaultSerializer = BsonSerializer.LookupSerializer(typeof(ReadOnlyCollection<Animal>));
            var bsonSerializer = (ReadOnlyCollectionSerializer<Animal>)defaultSerializer
                .AsDenormalizingBsonClassMapSerializer(DenormalizedMemberNames);
            Assert.NotNull(bsonSerializer);
            Assert.IsType(defaultSerializer.GetType(), bsonSerializer);
            Assert.IsType<DenormalizingBsonClassMapSerializer<Animal>>(bsonSerializer.ItemSerializer);

            var readOnlyCollection = new ReadOnlyCollection<Animal>(_zooEntities.Animals.ToList());
            IEnumerable<string> documents = readOnlyCollection
                .Select(animal => $"{{ \"_id\" : ObjectId(\"{animal.AnimalId}\"), {GetDiscriminator(animal)}, \"Age\" : \"{animal.Age}\", \"Height\" : \"{animal.Height}\", \"Weight\" : \"{animal.Weight}\" }}");
            string expectedDocument =
                $"[{string.Join(", ", documents)}]";

            Assert.Equal(expectedDocument, readOnlyCollection.ToJson(typeof(ReadOnlyCollection<Animal>), serializer: bsonSerializer));
        }

        [Fact]
        public void Can_denormalize_ReadOnlyCollection_sub_class()
        {
            IBsonSerializer defaultSerializer = BsonSerializer.LookupSerializer(typeof(ReadOnlyObservableCollection<Animal>));
            var bsonSerializer = (ReadOnlyCollectionSubclassSerializer< ReadOnlyObservableCollection<Animal>, Animal>)defaultSerializer
                .AsDenormalizingBsonClassMapSerializer(DenormalizedMemberNames);
            Assert.NotNull(bsonSerializer);
            Assert.IsType(defaultSerializer.GetType(), bsonSerializer);
            Assert.IsType<DenormalizingBsonClassMapSerializer<Animal>>(bsonSerializer.ItemSerializer);

            var observableCollection = new ObservableCollection<Animal>(_zooEntities.Animals.ToList());
            var readOnlyObservableCollection = new ReadOnlyObservableCollection<Animal>(observableCollection);
            IEnumerable<string> documents = readOnlyObservableCollection
                .Select(animal => $"{{ \"_id\" : ObjectId(\"{animal.AnimalId}\"), {GetDiscriminator(animal)}, \"Age\" : \"{animal.Age}\", \"Height\" : \"{animal.Height}\", \"Weight\" : \"{animal.Weight}\" }}");
            string expectedDocument =
                $"[{string.Join(", ", documents)}]";

            Assert.Equal(expectedDocument, readOnlyObservableCollection.ToJson(typeof(ReadOnlyObservableCollection<Animal>), serializer: bsonSerializer));
        }
    }
}
