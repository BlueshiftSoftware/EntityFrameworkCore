using System;
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
