using System;
using Blueshift.EntityFrameworkCore.MongoDB.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;

namespace Blueshift.Identity.MongoDB.Tests
{
    public class MongoDbIdentityFixture : IDisposable
    {
        private const string MongoUrl = "mongodb://localhost:27017";

        protected IServiceProvider ServiceProvider;
        private IdentityMongoDbContext _identityDbContext;

        public MongoDbIdentityFixture()
        {
            ServiceProvider = new ServiceCollection()
                .AddDbContext<IdentityMongoDbContext>(options => options
                    .UseMongoDb(
                        connectionString: MongoUrl,
                        mongoDbOptionsAction: optionsBuilder => optionsBuilder.UseDatabase("__test_identities"))
                    .EnableSensitiveDataLogging(true))
                .AddIdentity<MongoDbIdentityUser, MongoDbIdentityRole>()
                .AddEntityFrameworkMongoDbStores<IdentityMongoDbContext>()
                .Services
                .BuildServiceProvider();
            _identityDbContext = ServiceProvider.GetRequiredService<IdentityMongoDbContext>();
            _identityDbContext.Database.EnsureCreated();
        }

        public T GetService<T>()
            => ServiceProvider.GetRequiredService<T>();

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_identityDbContext != null)
                {
                    _identityDbContext.Database.EnsureDeleted();
                    _identityDbContext = null;
                }
            }
        }
    }
}