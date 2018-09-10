using System;
using System.Reflection;
using Blueshift.EntityFrameworkCore.MongoDB.Adapter;
using Blueshift.EntityFrameworkCore.MongoDB.Adapter.Conventions;
using MongoDB.Bson.Serialization.Conventions;
using Xunit;

namespace Blueshift.EntityFrameworkCore.MongoDB.Tests.Adapter
{
    public class EntityFrameworkConventionPackTests
    {
        [Theory]
        [InlineData(typeof(AbstractBaseClassConvention))]
        [InlineData(typeof(KeyAttributeConvention))]
        [InlineData(typeof(NavigationSrializationMemberMapConvention))]
        [InlineData(typeof(NotMappedAttributeConvention))]
        public void Singleton_contains_default_convention_set(Type conventionType)
        {
            ConventionPack conventionPack = EntityFrameworkConventionPack.Instance;
            Assert.Contains(conventionPack, conventionType.GetTypeInfo().IsInstanceOfType);
        }
    }
}