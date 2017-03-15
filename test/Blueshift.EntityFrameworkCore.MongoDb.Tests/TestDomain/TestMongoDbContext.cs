using Blueshift.EntityFrameworkCore.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Blueshift.EntityFrameworkCore.MongoDB.Tests.TestDomain
{
    [MongoDatabase(database: "testdb")]
    public class TestMongoDbContext : DbContext
    {
        public TestMongoDbContext(DbContextOptions<TestMongoDbContext> dbContextOptions)
            : base(dbContextOptions)
        {
        }

        public DbSet<SimpleRecord> SimpleRecords { get; private set; }

        public DbSet<ComplexRecord> ComplexRecords { get; private set; }

        public DbSet<RootType> RootTypes { get; private set; }
    }
}