using System;
using System.Reflection;
using Blueshift.EntityFrameworkCore.MongoDB.Adapter.Conventions;
using Blueshift.EntityFrameworkCore.MongoDB.SampleDomain;
using MongoDB.Bson.Serialization;
using Xunit;

namespace Blueshift.EntityFrameworkCore.MongoDB.Tests.Adapter.Conventions
{
    public class AbstractClassConventionTest
    {
        [Theory]
        [InlineData(typeof(Animal))]
        [InlineData(typeof(Tiger))]
        [InlineData(typeof(PolarBear))]
        [InlineData(typeof(Otter))]
        [InlineData(typeof(SeaOtter))]
        [InlineData(typeof(EurasianOtter))]
        [InlineData(typeof(Employee))]
        public void Sets_is_root_class_and_discriminator_required_true_for_abstract_type(Type type)
        {
            var classMap = new BsonClassMap(type);
            var abstractClassMapConvention = new AbstractClassConvention();
            abstractClassMapConvention.Apply(classMap);
            Assert.Equal(type.GetTypeInfo().IsAbstract, classMap.DiscriminatorIsRequired);
        }
    }
}