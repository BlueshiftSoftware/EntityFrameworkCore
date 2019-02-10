using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Blueshift.EntityFrameworkCore.MongoDB.Adapter.Conventions;
using Blueshift.EntityFrameworkCore.MongoDB.SampleDomain;
using MongoDB.Bson.Serialization;
using Xunit;

namespace Blueshift.EntityFrameworkCore.MongoDB.Tests.Adapter.Conventions
{
    public class KeyAttributeConventionTests
    {
        [Fact]
        public void Should_set_id_member_when_key_attribute_present()
        {
            MemberInfo memberInfo = typeof(Animal)
                .GetTypeInfo()
                .GetProperty(nameof(Animal.AnimalId));
            Assert.NotNull(memberInfo);
            Assert.True(memberInfo.IsDefined(typeof(KeyAttribute), false));
            var keyAttributeConvention = new KeyAttributeConvention();
            var bsonClasspMap = new BsonClassMap<Animal>();
            BsonMemberMap bsonMemberMap = bsonClasspMap.MapMember(memberInfo);
            keyAttributeConvention.Apply(bsonMemberMap);
            Assert.Same(bsonMemberMap, bsonClasspMap.IdMemberMap);
        }

        [Theory]
        [InlineData(nameof(Employee.FirstName))]
        [InlineData(nameof(Employee.Age))]
        public void Should_not_set_id_member_when_key_attribute_present(string memberName)
        {
            MemberInfo memberInfo = typeof(Employee)
                .GetTypeInfo()
                .GetProperty(memberName);
            Assert.NotNull(memberInfo);
            Assert.False(memberInfo.IsDefined(typeof(KeyAttribute), false));
            var keyAttributeConvention = new KeyAttributeConvention();
            var bsonClasspMap = new BsonClassMap<Employee>();
            BsonMemberMap bsonMemberMap = bsonClasspMap.MapMember(memberInfo);
            keyAttributeConvention.Apply(bsonMemberMap);
            Assert.NotSame(bsonMemberMap, bsonClasspMap.IdMemberMap);
        }
    }
}