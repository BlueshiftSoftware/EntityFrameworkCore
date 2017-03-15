using Blueshift.EntityFrameworkCore.MongoDB.Adapter;
using Blueshift.EntityFrameworkCore.MongoDB.Tests.TestDomain;
using MongoDB.Bson.Serialization;
using Xunit;

namespace Blueshift.EntityFrameworkCore.MongoDB.Tests.MongoDB.Adapter
{
    public class AbstractClassMapConventionTest
    {
        [Fact]
        public void Sets_is_root_class_and_discriminator_required_true_for_abstract_type()
        {
            var classMap = new BsonClassMap<RootType>();
            var abstractClassMapConvention = new AbstractClassMapConvention();
            abstractClassMapConvention.Apply(classMap);
            Assert.True(classMap.DiscriminatorIsRequired);
        }

        [Fact]
        public void Ignores_concrete_type()
        {
            var classMap = new BsonClassMap<SimpleRecord>();
            var abstractClassMapConvention = new AbstractClassMapConvention();
            abstractClassMapConvention.Apply(classMap);
            Assert.False(classMap.DiscriminatorIsRequired);
        }
    }
}