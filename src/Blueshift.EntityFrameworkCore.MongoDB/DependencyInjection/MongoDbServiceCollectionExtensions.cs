using Blueshift.EntityFrameworkCore.MongoDB.Infrastructure;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class MongoDbServiceCollectionExtensions
    {
        public static IServiceCollection AddEntityFrameworkMongoDb([NotNull] this IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            var entityFrameworkServicesBuilder = new EntityFrameworkMongoDbServicesBuilder(serviceCollection);
            entityFrameworkServicesBuilder.TryAddCoreServices();
            return serviceCollection;
        }
    }
}