using System;
using Blueshift.MongoDB.Tests.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Blueshift.Identity.MongoDB.Tests
{
    public class MongoDbIdentityFixture : MongoDbFixtureBase
    {
        public IServiceProvider Services = new ServiceCollection()
            .AddDbContext<IdentityMongoDbContext>(options => options.UseMongoDb(connectionString: MongoDbConstants.MongoUrl))
            .AddIdentity<MongoDbIdentityUser, MongoDbIdentityRole>()
            .AddEntityFrameworkMongoDbStores<IdentityMongoDbContext>()
            .Services
            .BuildServiceProvider();
    }
}
