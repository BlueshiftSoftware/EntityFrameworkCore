using Blueshift.EntityFrameworkCore.MongoDB.SampleDomain;
using Blueshift.MongoDB.Tests.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Blueshift.EntityFrameworkCore.MongoDB.Tests
{
    public class MongoDbFixture : MongoDbFixtureBase
    {
        public ZooDbContext ZooDbContext => new ServiceCollection()
                .AddDbContext<ZooDbContext>(options => options.UseMongoDb(connectionString: MongoDbConstants.MongoUrl))
                .BuildServiceProvider()
                .GetService<ZooDbContext>();
    }
}