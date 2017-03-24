using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace Blueshift.EntityFrameworkCore.MongoDB.SampleDomain
{
    public static class ZooDbDependencyInjection
    {
        public static IServiceCollection AddZooDbContext(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddDbContext<ZooDbContext>(options => options.UseMongoDb("mongodb://localhost"));
        }
    }
}