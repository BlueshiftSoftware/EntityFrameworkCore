using System.Reflection;
using Blueshift.EntityFrameworkCore.MongoDB.Tests.TestDomain;
using Blueshift.EntityFrameworkCore.ValueGeneration;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Xunit;

namespace Blueshift.EntityFrameworkCore.MongoDB.Tests.ValueGeneration
{
    public class MongoDbValueGeneratorSelectorTests
    {
        [Fact]
        public void X()
        {
            var model = new Model();
            EntityType entityType = model.AddEntityType(typeof(SimpleRecord));
            Property property = entityType.AddProperty(typeof(SimpleRecord)
                .GetTypeInfo()
                .GetProperty(nameof(SimpleRecord.Id)));

            var valueGeneratorCacheDependencies = new ValueGeneratorCacheDependencies();
            var valueGeneratorCache = new ValueGeneratorCache(valueGeneratorCacheDependencies);
            var valueGeneratorSelectorDependencies = new ValueGeneratorSelectorDependencies(valueGeneratorCache);
            var mongoDbValueGeneratorSelector = new MongoDbValueGeneratorSelector(valueGeneratorSelectorDependencies);
            ValueGenerator valueGenerator = mongoDbValueGeneratorSelector.Select(property, entityType);
            Assert.IsAssignableFrom(typeof(ObjectIdValueGenerator), valueGenerator);
        }
    }
}