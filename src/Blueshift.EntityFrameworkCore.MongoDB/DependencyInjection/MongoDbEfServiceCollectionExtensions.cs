using System.ComponentModel;
using System.Reflection;
using Blueshift.EntityFrameworkCore.MongoDB;
using Blueshift.EntityFrameworkCore.MongoDB.Adapter;
using Blueshift.EntityFrameworkCore.MongoDB.Infrastructure;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Bson;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extends <see cref="IServiceCollection"/> with methods for use with the MongoDb EntityFrameworkCore provider.
    /// </summary>
    public static class MongoDbEfServiceCollectionExtensions
    {
        static MongoDbEfServiceCollectionExtensions()
        {
            if (!typeof(ObjectId).GetTypeInfo().IsDefined(typeof(TypeConverterAttribute)))
            {
                TypeDescriptor.AddAttributes(typeof(ObjectId), new TypeConverterAttribute(typeof(ObjectIdTypeConverter)));
            }

            EntityFrameworkConventionPack.Register(type => true);
        }

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