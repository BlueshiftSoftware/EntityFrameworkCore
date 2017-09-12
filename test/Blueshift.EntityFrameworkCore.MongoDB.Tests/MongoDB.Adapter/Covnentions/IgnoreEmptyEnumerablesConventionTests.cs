using Blueshift.EntityFrameworkCore.MongoDB.Adapter.Conventions;
using Blueshift.EntityFrameworkCore.MongoDB.SampleDomain;
using MongoDB.Bson.Serialization;
using Xunit;

namespace Blueshift.EntityFrameworkCore.MongoDB.Tests.MongoDB.Adapter.Conventions
{
    public class IgnoreEmptyEnumerablesConventionTests
    {
        [Fact]
        public void Should_not_serialize_empty_enumerables()
        {
            var bsonClassMap = new BsonClassMap<Employee>();
            BsonMemberMap bsonMemberMap = bsonClassMap.MapMember(e => e.Specialties);
            var ignoreEmptyEnumerableConvention = new IgnoreEmptyEnumerablesConvention();
            ignoreEmptyEnumerableConvention.Apply(bsonMemberMap);
            var employee = new Employee();
            employee.Specialties.Clear();
            Assert.False(bsonMemberMap.ShouldSerialize(employee, employee.Specialties));
        }

        [Fact]
        public void Should_serialize_non_empty_enumerables()
        {
            var bsonClassMap = new BsonClassMap<Employee>();
            BsonMemberMap bsonMemberMap = bsonClassMap.MapMember(e => e.Specialties);
            var ignoreEmptyEnumerableConvention = new IgnoreEmptyEnumerablesConvention();
            ignoreEmptyEnumerableConvention.Apply(bsonMemberMap);
            var employee = new Employee
            {
                Specialties =
                {
                    new Specialty { AnimalType = nameof(Tiger), Task = ZooTask.Feeding },
                    new Specialty { AnimalType = nameof(PolarBear), Task = ZooTask.Feeding },
                    new Specialty { AnimalType = nameof(Otter), Task = ZooTask.Feeding }
                }
            };
            Assert.True(bsonMemberMap.ShouldSerialize(employee, employee.Specialties));
        }
    }
}