using System.Reflection;
using Blueshift.EntityFrameworkCore.MongoDB.SampleDomain;
using Blueshift.EntityFrameworkCore.MongoDB.ValueGeneration;
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
            EntityType entityType = model.AddEntityType(typeof(Employee));
            Property property = entityType.AddProperty(typeof(Employee)
                .GetTypeInfo()
                .GetProperty(nameof(Employee.Id)));

            var valueGeneratorCacheDependencies = new ValueGeneratorCacheDependencies();
            var valueGeneratorCache = new ValueGeneratorCache(valueGeneratorCacheDependencies);
            var valueGeneratorSelectorDependencies = new ValueGeneratorSelectorDependencies(valueGeneratorCache);
            var mongoDbValueGeneratorSelector = new MongoDbValueGeneratorSelector(valueGeneratorSelectorDependencies);
            ValueGenerator valueGenerator = mongoDbValueGeneratorSelector.Select(property, entityType);
            Assert.IsAssignableFrom<ObjectIdValueGenerator>(valueGenerator);
        }
    }
}