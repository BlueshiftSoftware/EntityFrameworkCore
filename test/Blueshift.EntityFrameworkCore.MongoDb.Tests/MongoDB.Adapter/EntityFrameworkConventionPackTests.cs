using System;
using System.Reflection;
using Blueshift.EntityFrameworkCore.Annotations;
using Blueshift.EntityFrameworkCore.MongoDB.Adapter;
using MongoDB.Bson.Serialization.Conventions;
using Xunit;

namespace Blueshift.EntityFrameworkCore.MongoDB.Tests.MongoDB.Adapter
{
    public class EntityFrameworkConventionPackTests
    {
        [Theory]
        [InlineData(typeof(AbstractClassMapConvention))]
        [InlineData(typeof(BsonClassMapAttributeConvention<DerivedTypeAttribute>))]
        [InlineData(typeof(KeyAttributeConvention))]
        [InlineData(typeof(IgnoreEmptyEnumerablesConvention))]
        [InlineData(typeof(IgnoreNullOrEmptyStringsConvention))]
        public void Singleton_contains_default_convention_set(Type conventionType)
        {
            ConventionPack conventionPack = EntityFrameworkConventionPack.Instance;
            Assert.Contains(conventionPack, conventionType.GetTypeInfo().IsInstanceOfType);
        }
    }
}