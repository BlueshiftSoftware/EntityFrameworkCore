using System;
using System.Collections.Generic;
using Blueshift.EntityFrameworkCore.MongoDB.SampleDomain;
using Blueshift.EntityFrameworkCore.MongoDB.Storage;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MongoDB.Bson;
using Moq;
using Xunit;

namespace Blueshift.EntityFrameworkCore.MongoDB.Tests.Storage
{
    public class MongoDbTypeMappingSourceTests
    {
        private readonly MongoDbTypeMappingSource _mongoDbTypeMappingSource = new MongoDbTypeMappingSource(
            new TypeMappingSourceDependencies(
                Mock.Of<IValueConverterSelector>()));

        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(long))]
        [InlineData(typeof(short))]
        [InlineData(typeof(byte))]
        [InlineData(typeof(uint))]
        [InlineData(typeof(ulong))]
        [InlineData(typeof(ushort))]
        [InlineData(typeof(sbyte))]
        [InlineData(typeof(char))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(byte[]))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(DateTimeOffset))]
        [InlineData(typeof(decimal))]
        [InlineData(typeof(double))]
        [InlineData(typeof(float))]
        [InlineData(typeof(Guid))]
        [InlineData(typeof(string))]
        [InlineData(typeof(TimeSpan))]
        [InlineData(typeof(ZooTask))]
        [InlineData(typeof(ObjectId))]
        [InlineData(typeof(IEnumerable<int>))]
        [InlineData(typeof(IEnumerable<long>))]
        [InlineData(typeof(IEnumerable<short>))]
        [InlineData(typeof(IEnumerable<byte>))]
        [InlineData(typeof(IEnumerable<uint>))]
        [InlineData(typeof(IEnumerable<ulong>))]
        [InlineData(typeof(IEnumerable<ushort>))]
        [InlineData(typeof(IEnumerable<sbyte>))]
        [InlineData(typeof(IEnumerable<char>))]
        [InlineData(typeof(IEnumerable<bool>))]
        [InlineData(typeof(IEnumerable<byte[]>))]
        [InlineData(typeof(IEnumerable<DateTime>))]
        [InlineData(typeof(IEnumerable<DateTimeOffset>))]
        [InlineData(typeof(IEnumerable<decimal>))]
        [InlineData(typeof(IEnumerable<double>))]
        [InlineData(typeof(IEnumerable<float>))]
        [InlineData(typeof(IEnumerable<ObjectId>))]
        [InlineData(typeof(IEnumerable<Guid>))]
        [InlineData(typeof(IEnumerable<string>))]
        [InlineData(typeof(IEnumerable<TimeSpan>))]
        [InlineData(typeof(IEnumerable<ZooTask>))]
        [InlineData(typeof(IDictionary<string, ZooTask>))]
        public void Primitives_and_enumerables_of_primitives_are_mapped(Type type)
        {
            Assert.NotNull(_mongoDbTypeMappingSource.FindMapping(type));
        }

        [Theory]
        [InlineData(typeof(Employee))]
        [InlineData(typeof(Animal))]
        [InlineData(typeof(Tiger))]
        [InlineData(typeof(PolarBear))]
        [InlineData(typeof(Otter))]
        [InlineData(typeof(SeaOtter))]
        [InlineData(typeof(EurasianOtter))]
        [InlineData(typeof(Specialty))]
        [InlineData(typeof(IEnumerable<Specialty>))]
        [InlineData(typeof(IDictionary<string, Specialty>))]
        public void Entities_and_complex_types_are_not_mapped(Type type)
        {
            Assert.Null(_mongoDbTypeMappingSource.FindMapping(type));
        }
    }
}
