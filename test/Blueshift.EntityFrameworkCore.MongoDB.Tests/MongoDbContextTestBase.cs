using System;
using Blueshift.EntityFrameworkCore.MongoDB.SampleDomain;
using Blueshift.MongoDB.Tests.Shared;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Blueshift.EntityFrameworkCore.MongoDB.Tests
{
    public abstract class MongoDbContextTestBase : IClassFixture<MongoDbFixture>, IDisposable
    {
        protected IServiceProvider ServiceProvider;
        protected ZooDbContext ZooDbContext;

        protected MongoDbContextTestBase([UsedImplicitly] MongoDbFixture mongoDbFixture)
        {
            ServiceProvider = new ServiceCollection()
                .AddDbContext<ZooDbContext>(options => options
                    .UseMongoDb(connectionString: MongoDbConstants.MongoUrl)
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