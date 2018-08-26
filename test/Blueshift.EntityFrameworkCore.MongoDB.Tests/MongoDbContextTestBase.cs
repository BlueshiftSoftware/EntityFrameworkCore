using System;
using Blueshift.EntityFrameworkCore.MongoDB.SampleDomain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Blueshift.EntityFrameworkCore.MongoDB.Tests
{
    public abstract class MongoDbContextTestBase : IDisposable
    {
        private const string MongoUrl = "mongodb://localhost:27017";

        protected IServiceProvider ServiceProvider;
        protected ZooDbContext ZooDbContext;

        protected MongoDbContextTestBase()
        {
            ServiceProvider = new ServiceCollection()
                .AddDbContext<ZooDbContext>(options => options
                    .UseMongoDb(MongoUrl)
                    .EnableSensitiveDataLogging(true))
                .BuildServiceProvider();
            ZooDbContext = ServiceProvider.GetService<ZooDbContext>();
            ZooDbContext.Database.EnsureCreated();
        }

        public void Dispose()
        {
            if (ZooDbContext != null)
            {
                ZooDbContext.Database.EnsureDeleted();
                ZooDbContext.Dispose();
                ZooDbContext = null;
            }
        }
    }
}