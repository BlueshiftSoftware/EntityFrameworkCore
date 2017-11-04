using System.Collections.Generic;
using System.Linq;
using Blueshift.EntityFrameworkCore.MongoDB.Adapter.Serialization;
using Blueshift.EntityFrameworkCore.MongoDB.SampleDomain;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using Xunit;

namespace Blueshift.EntityFrameworkCore.MongoDB.Tests.Adapter.Serialization
{
    public class NavigationBsonSerializerTests
    {
        private static readonly string[] DenormalizedMemberNames =
        {
            nameof(Animal.Age),
            nameof(Animal.Height),
            nameof(Animal.Weight)
        };

        private static readonly IEnumerable<BsonMemberMap> DenormalizedMemberMaps
            = BsonClassMap.LookupClassMap(typeof(Animal))
                .AllMemberMaps
                .Where(bsonMemberMap => DenormalizedMemberNames.Contains(bsonMemberMap.MemberName))
                .ToList();

        [Fact]
        public void Correctly_Serializes_Entity_Reference()
        {
            var animalNavigationSerializer = new NavigationBsonSerializer<Animal>();
            Tiger tigger = TestEntityFixture.Tigger;
            string tiggerJson = tigger.ToJson(typeof(Animal), serializer: animalNavigationSerializer);
            Assert.Equal($"{{ \"_id\" : ObjectId(\"{tigger.Id}\"), \"_t\" : [\"Animal\", \"panthera tigris\"] }}", tiggerJson);
            Assert.DoesNotContain($"\"Name\": \"${tigger.Name}\"", tiggerJson);
            Assert.DoesNotContain($"\"Age\": \"${tigger.Age}\"", tiggerJson);
            Assert.DoesNotContain($"\"Height\": \"${tigger.Height}\"", tiggerJson);
            Assert.DoesNotContain($"\"Weight\": \"${tigger.Weight}\"", tiggerJson);
        }

        [Fact]
        public void Correctly_Serializes_Entity_Reference_With_Denormalized_Properties()
        {
            var animalNavigationSerializer = new NavigationBsonSerializer<Animal>(DenormalizedMemberMaps);
            Animal tigger = TestEntityFixture.Tigger;
            string tiggerJson = tigger.ToJson(typeof(Animal), serializer: animalNavigationSerializer);
            Assert.Equal($"{{ \"_id\" : ObjectId(\"{tigger.Id}\"), \"_t\" : [\"Animal\", \"panthera tigris\"], \"Age\" : \"{tigger.Age}\", \"Height\" : \"{tigger.Height}\", \"Weight\" : \"{tigger.Weight}\" }}", tiggerJson);
            Assert.DoesNotContain($"\"Name\": \"${tigger.Name}\"", tiggerJson);
        }

        [Fact]
        public void Deserialize_uses_default_deserializer()
        {
            var animalNavigationSerializer = new NavigationBsonSerializer<Animal>();
            Animal tigger = TestEntityFixture.Tigger;
            BsonDocument bsonDocument = tigger.ToBsonDocument();
            Animal animal;
            using (var bsonReader = new BsonDocumentReader(bsonDocument))
            {
                BsonDeserializationContext bsonDeserializationContext = BsonDeserializationContext.CreateRoot(bsonReader);
                var bsonDeserializationArgs = new BsonDeserializationArgs() { NominalType = typeof(Animal) };
                animal = animalNavigationSerializer.Deserialize(bsonDeserializationContext, bsonDeserializationArgs);
            }
            Assert.Equal(tigger, animal, new AnimalEqualityComparer());
        }

        [Fact]
        public void Deserialize_can_deserialize_partial_class()
        {
            var animalNavigationSerializer = new NavigationBsonSerializer<Animal>();
            Animal tigger = TestEntityFixture.Tigger;
            BsonDocument bsonDocument = tigger.ToBsonDocument(serializer: animalNavigationSerializer);
            Animal animal;
            using (var bsonReader = new BsonDocumentReader(bsonDocument))
            {
                BsonDeserializationContext bsonDeserializationContext = BsonDeserializationContext.CreateRoot(bsonReader);
                var bsonDeserializationArgs = new BsonDeserializationArgs() { NominalType = typeof(Animal) };
                animal = animalNavigationSerializer.Deserialize(bsonDeserializationContext, bsonDeserializationArgs);
            }
            Assert.NotNull(animal);
            Assert.IsType<Tiger>(animal);
            Assert.Equal(tigger.Id, animal.Id);
            Assert.Null(animal.Name);
            Assert.Equal(0, animal.Age);
            Assert.Equal(0, animal.Height);
            Assert.Equal(0, animal.Weight);
        }
    }
}
