using System;
using System.Threading.Tasks;
using Blueshift.EntityFrameworkCore.MongoDB.Infrastructure;
using Blueshift.EntityFrameworkCore.MongoDB.SampleDomain;
using Microsoft.Extensions.DependencyInjection;

namespace Blueshift.EntityFrameworkCore.MongoDB.Tests
{
    public abstract class MongoDbContextTestBase : IDisposable
    {
        private const string MongoUrl = "mongodb://localhost:27017";

        protected IServiceProvider ServiceProvider;

        protected MongoDbContextTestBase()
        {
            ServiceProvider = new ServiceCollection()
                .AddDbContext<ZooDbContext>(options => options
                    .UseMongoDb(MongoUrl, mongoDbContextOptionsBuilder =>
                        mongoDbContextOptionsBuilder.EnableQueryLogging())
                    .EnableSensitiveDataLogging(true))
                .BuildServiceProvider();

            ExecuteUnitOfWork(zooDbContext => zooDbContext.Database.EnsureCreated());
        }

        public void Dispose()
        {
            ExecuteUnitOfWork(zooDbContext => zooDbContext.Database.EnsureDeleted());
        }

        protected void ExecuteUnitOfWork(Action<ZooDbContext> unitOfWork)
        {
            using (IServiceScope serviceScope = ServiceProvider.CreateScope())
            {
                ZooDbContext zooDbContext = serviceScope.ServiceProvider.GetService<ZooDbContext>();
                unitOfWork(zooDbContext);
            }
        }

        protected async Task ExecuteUnitOfWorkAsync(Func<ZooDbContext, Task> unitOfWork)
        {
            using (IServiceScope serviceScope = ServiceProvider.CreateScope())
            {
                ZooDbContext zooDbContext = serviceScope.ServiceProvider.GetService<ZooDbContext>();
                await unitOfWork(zooDbContext);
            }
        }

        protected async Task<TResult> ExecuteUnitOfWorkAsync<TResult>(Func<ZooDbContext, Task<TResult>> unitOfWork)
        {
            using (IServiceScope serviceScope = ServiceProvider.CreateScope())
            {
                ZooDbContext zooDbContext = serviceScope.ServiceProvider.GetService<ZooDbContext>();
                return await unitOfWork(zooDbContext);
            }
        }
    }
}