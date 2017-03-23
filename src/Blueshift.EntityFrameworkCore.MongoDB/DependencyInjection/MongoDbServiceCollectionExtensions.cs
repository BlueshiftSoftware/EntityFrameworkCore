using Blueshift.EntityFrameworkCore.MongoDB.Infrastructure;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extends <see cref="IServiceCollection"/> with methods for use with the MongoDb EntityFrameworkCore provider.
    /// </summary>
    public static class MongoDbServiceCollectionExtensions
    {
        /// <summary>
        /// Populates the given <paramref name="serviceCollection"/> instance with the service dependencies for
        /// the MongoDb provider for EntityFrameworkCore.
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/> instance of populate.</param>
        /// <returns>The <paramref name="serviceCollection"/> populated with the MongoDb EntityFrameworkCore dependencies.</returns>
        public static IServiceCollection AddEntityFrameworkMongoDb([NotNull] this IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            var entityFrameworkServicesBuilder = new EntityFrameworkMongoDbServicesBuilder(serviceCollection);
            entityFrameworkServicesBuilder.TryAddCoreServices();
            return serviceCollection;
        }
    }
}