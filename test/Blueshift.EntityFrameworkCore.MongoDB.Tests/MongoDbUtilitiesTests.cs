using System;
using Blueshift.EntityFrameworkCore.MongoDB.SampleDomain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace Blueshift.EntityFrameworkCore.MongoDB.Tests
{
    public class MongoDbUtilitiesTests
    {
        [Theory]
        [InlineData("monkey", "monkeys")]
        [InlineData("laptop", "laptops")]
        [InlineData("cpu", "cpus")]
        [InlineData("horse", "horses")]
        [InlineData("pony", "ponies")]
        public void Pluralize_singular_strings(string value, string expected)
            => Assert.Equal(expected, MongoDbUtilities.Pluralize(value));

        [Theory]
        [InlineData("monkeys")]
        [InlineData("horses")]
        [InlineData("ponies")]
        public void Pluralize_does_not_change_plurals(string value)
            => Assert.Equal(value, MongoDbUtilities.Pluralize(value));

        [Theory]
        [InlineData("CPU", "cpu")]
        [InlineData("ETA", "eta")]
        [InlineData("EPA", "epa")]
        [InlineData("TLA", "tla")]
        public void Camel_case_uppercase_strings(string value, string expected)
            => Assert.Equal(expected, MongoDbUtilities.ToLowerCamelCase(value));

        [Theory]
        [InlineData("EFTests", "efTests")]
        [InlineData("NYCity", "nyCity")]
        [InlineData("TLAcronym", "tlAcronym")]
        [InlineData("ThreeLetterAcronym", "threeLetterAcronym")]
        public void Camel_case_does_not_change_trailing_words(string value, string expected)
            => Assert.Equal(expected, MongoDbUtilities.ToLowerCamelCase(value));

        [Theory]
        [InlineData(typeof(Animal), typeof(Animal))]
        [InlineData(typeof(Tiger), typeof(Animal))]
        [InlineData(typeof(PolarBear), typeof(Animal))]
        [InlineData(typeof(Otter), typeof(Animal))]
        [InlineData(typeof(EurasianOtter), typeof(Animal))]
        [InlineData(typeof(SeaOtter), typeof(Animal))]
        [InlineData(typeof(Employee), typeof(Employee))]
        [InlineData(typeof(Enclosure), typeof(Enclosure))]
        public void GetCollectionEntityType_returns_least_derived_entity_type(Type documentType, Type expectedType)
        {
            var zooDbContext = new ZooDbContext(new DbContextOptions<ZooDbContext>());
            IEntityType documentEntityType = zooDbContext.Model.FindEntityType(documentType);
            IEntityType expectedEntityType = zooDbContext.Model.FindEntityType(expectedType);

            Assert.Equal(expectedEntityType, documentEntityType.GetMongoDbCollectionEntityType());
        }

        [Theory]
        [InlineData(typeof(Animal))]
        [InlineData(typeof(Tiger))]
        [InlineData(typeof(PolarBear))]
        [InlineData(typeof(Otter))]
        [InlineData(typeof(EurasianOtter))]
        [InlineData(typeof(SeaOtter))]
        [InlineData(typeof(Employee))]
        [InlineData(typeof(Enclosure))]
        public void IsDocumentRootEntityType_returns_true_for_root_entity_types(Type documentType)
        {
            var zooDbContext = new ZooDbContext(new DbContextOptions<ZooDbContext>());
            IEntityType documentEntityType = zooDbContext.Model.FindEntityType(documentType);

            Assert.True(documentEntityType.IsDocumentRootEntityType());
        }

        [Theory]
        [InlineData(typeof(Schedule))]
        [InlineData(typeof(Specialty))]
        [InlineData(typeof(ZooAssignment))]
        public void IsDocumentRootEntityType_returns_false_for_owned_entity_types(Type documentType)
        {
            var zooDbContext = new ZooDbContext(new DbContextOptions<ZooDbContext>());
            IEntityType documentEntityType = zooDbContext.Model.FindEntityType(documentType);

            Assert.False(documentEntityType.IsDocumentRootEntityType());
        }
    }
}