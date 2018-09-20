using System;
using Blueshift.EntityFrameworkCore.MongoDB.Annotations;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Blueshift.EntityFrameworkCore.MongoDB.Tests.Metadata.Conventions
{
    public class MongoDatabaseConventionTests
    {
        [Theory]
        [InlineData(typeof(ZooDb), "zoo")]
        [InlineData(typeof(ZooDbContext), "zoo")]
        [InlineData(typeof(ZooContext), "zoo")]
        [InlineData(typeof(ZooMongo), "zoo")]
        [InlineData(typeof(ZooMongoDb), "zoo")]
        [InlineData(typeof(ZooMongoDbContext), "zoo")]
        [InlineData(typeof(ZooMongoContext), "zoo")]
        [InlineData(typeof(AnnodatedZooContext), "zoo")]
        [InlineData(typeof(DifferentlyAnnodatedZooContext), "zhou")]
        public void Should_set_expected_database_name(Type dbContextType, string expectedName)
        {
            DbContext dbContext = (DbContext) Activator.CreateInstance(dbContextType);
            Assert.Equal(expectedName, dbContext.Model.MongoDb().Database);
        }

        private class MongoDatabaseAttributeDbContextBase : DbContext
        {
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder
                    .UseMongoDb("mongodb://localhost:27017");
                base.OnConfiguring(optionsBuilder);
            }
        }

        private class ZooDb : MongoDatabaseAttributeDbContextBase { }

        private class ZooDbContext : MongoDatabaseAttributeDbContextBase { }

        private class ZooContext : MongoDatabaseAttributeDbContextBase { }

        private class ZooMongo : MongoDatabaseAttributeDbContextBase { }

        private class ZooMongoDb : MongoDatabaseAttributeDbContextBase { }

        private class ZooMongoDbContext : MongoDatabaseAttributeDbContextBase { }

        private class ZooMongoContext : MongoDatabaseAttributeDbContextBase { }

        [MongoDatabase("zoo")]
        private class AnnodatedZooContext : MongoDatabaseAttributeDbContextBase { }

        [MongoDatabase("zhou")]
        private class DifferentlyAnnodatedZooContext : MongoDatabaseAttributeDbContextBase { }
    }
}
