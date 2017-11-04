using System;
using System.Collections.Generic;
using Blueshift.EntityFrameworkCore.MongoDB.SampleDomain;
using Blueshift.EntityFrameworkCore.MongoDB.Storage;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace Blueshift.EntityFrameworkCore.MongoDB.Tests.Storage
{
    public class MongoDbTypeMapperTests
    {
        private readonly ITypeMapper _typeMapper = new MongoDbTypeMapper();

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
        [InlineData(typeof(Specialty))]
        public void Primitives_and_complex_types_are_mapped(Type type)
        {
            Assert.True(_typeMapper.IsTypeMapped(type));
        }

        [Theory]
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
        [InlineData(typeof(IEnumerable<Guid>))]
        [InlineData(typeof(IEnumerable<string>))]
        [InlineData(typeof(IEnumerable<TimeSpan>))]
        [InlineData(typeof(IEnumerable<ZooTask>))]
        [InlineData(typeof(IEnumerable<Specialty>))]
        [InlineData(typeof(IDictionary<string, ZooTask>))]
        [InlineData(typeof(IDictionary<string, Specialty>))]
        public void Enumerables_of_primitives_and_complex_types_are_mapped(Type type)
        {
            Assert.True(_typeMapper.IsTypeMapped(type));
        }

        [Theory]
        [InlineData(typeof(Employee))]
        [InlineData(typeof(Animal))]
        [InlineData(typeof(Tiger))]
        [InlineData(typeof(PolarBear))]
        [InlineData(typeof(Otter))]
        [InlineData(typeof(SeaOtter))]
        [InlineData(typeof(EurasianOtter))]
        public void Entities_are_not_mapped(Type type)
        {
            Assert.False(_typeMapper.IsTypeMapped(type));
        }
    }
}
