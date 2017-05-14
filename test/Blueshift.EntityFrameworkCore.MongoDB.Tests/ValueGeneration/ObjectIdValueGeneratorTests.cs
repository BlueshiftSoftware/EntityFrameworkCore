using Blueshift.EntityFrameworkCore.MongoDB.ValueGeneration;
using MongoDB.Bson;
using Xunit;

namespace Blueshift.EntityFrameworkCore.MongoDB.Tests.ValueGeneration
{
    public class ObjectIdValueGeneratorTests
    {
        [Fact]
        public void Generates_temporary_values_returns_false()
        {
            var objectIdValueGenerator = new ObjectIdValueGenerator();
            Assert.False(objectIdValueGenerator.GeneratesTemporaryValues);
        }

        [Fact]
        public void Does_not_generate_empty_object_id()
        {
            var objectIdValueGenerator = new ObjectIdValueGenerator();
            Assert.NotEqual(ObjectId.Empty, objectIdValueGenerator.Next(entry: null));
        }

        [Fact]
        public void Generates_unique_object_ids()
        {
            var objectIdValueGenerator = new ObjectIdValueGenerator();
            ObjectId id = ObjectId.Empty;
            for (var i = 0; i < 100; i++)
            {
                Assert.NotEqual(id, id = objectIdValueGenerator.Next(entry: null));
            }
        }
    }
}