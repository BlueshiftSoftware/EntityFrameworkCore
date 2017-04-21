using System;
using Blueshift.EntityFrameworkCore.MongoDB.SampleDomain;
using Blueshift.MongoDB.Tests.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Blueshift.EntityFrameworkCore.MongoDB.Tests
{
    public abstract class MongoDbContextTestBase : IClassFixture<MongoDbFixture>, IDisposable
    {
        protected IServiceProvider _serviceProvider;
        protected ZooDbContext _zooDbContext;

        protected MongoDbContextTestBase(MongoDbFixture mongoDbFixture)
        {
            _serviceProvider = new ServiceCollection()
                .AddDbContext<ZooDbContext>(options => options
                    .UseMongoDb(connectionString: MongoDbConstants.MongoUrl)
                    .EnableSensitiveDataLogging(true))
                .BuildServiceProvider();
            _zooDbContext = _serviceProvider.GetService<ZooDbContext>();
            _zooDbContext.Database.EnsureCreated();
        }

        public void Dispose()
        {
            if (_zooDbContext != null)
            {
                _zooDbContext.Database.EnsureDeleted();
                _zooDbContext.Dispose();
                _zooDbContext = null;
            }
        }
    }
}