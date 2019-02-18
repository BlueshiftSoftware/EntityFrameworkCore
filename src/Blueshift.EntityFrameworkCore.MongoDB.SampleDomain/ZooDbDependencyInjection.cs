using Blueshift.EntityFrameworkCore.MongoDB.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Blueshift.EntityFrameworkCore.MongoDB.SampleDomain
{
    public static class ZooDbDependencyInjection
    {
        public static IServiceCollection AddZooDbContext(
            this IServiceCollection serviceCollection,
            string mongoUrl = "mongodb://localhost")
            => serviceCollection
                .AddDbContext<ZooDbContext>(options => options
                    .UseMongoDb(mongoUrl, mongoDbContextOptionsBuilder =>
                        mongoDbContextOptionsBuilder.EnableQueryLogging())
                    .EnableSensitiveDataLogging(true));
    }
}