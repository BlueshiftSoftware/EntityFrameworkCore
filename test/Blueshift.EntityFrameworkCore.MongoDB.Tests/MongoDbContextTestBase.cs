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
                .AddZooDbContext(MongoUrl)
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
                unitOfWork(serviceScope.ServiceProvider.GetService<ZooDbContext>());
            }
        }

        protected async Task ExecuteUnitOfWorkAsync(Func<ZooDbContext, Task> unitOfWork)
        {
            using (IServiceScope serviceScope = ServiceProvider.CreateScope())
            {
                await unitOfWork(serviceScope.ServiceProvider.GetService<ZooDbContext>());
            }
        }

        protected async Task<TResult> ExecuteUnitOfWorkAsync<TResult>(Func<ZooDbContext, Task<TResult>> unitOfWork)
        {
            using (IServiceScope serviceScope = ServiceProvider.CreateScope())
            {
                return await unitOfWork(serviceScope.ServiceProvider.GetService<ZooDbContext>());
            }
        }
    }
}