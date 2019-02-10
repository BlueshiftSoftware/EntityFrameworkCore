using System;
using System.Collections.Generic;
using Blueshift.EntityFrameworkCore.MongoDB.Adapter.Update;
using Blueshift.EntityFrameworkCore.MongoDB.Metadata.Builders;
using Blueshift.EntityFrameworkCore.MongoDB.Query;
using Blueshift.EntityFrameworkCore.MongoDB.Query.Expressions;
using Blueshift.EntityFrameworkCore.MongoDB.Query.ExpressionVisitors;
using Blueshift.EntityFrameworkCore.MongoDB.Storage;
using Blueshift.EntityFrameworkCore.MongoDB.ValueGeneration;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Blueshift.EntityFrameworkCore.MongoDB.Infrastructure
{
    /// <inheritdoc />
    /// <summary>
    ///     A builder API that populates an <see cref="IServiceCollection" /> with a set of EntityFrameworkCore
    ///     provider dependencies for MongoDb.
    /// </summary>
    public class EntityFrameworkMongoDbServicesBuilder : EntityFrameworkServicesBuilder
    {
        private static readonly IDictionary<Type, ServiceCharacteristics> RelationalServices
            = new Dictionary<Type, ServiceCharacteristics>
            {
                { typeof(IMongoClient), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IMongoDbConnection), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(IMongoDbTypeMappingSource), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(MongoDbEntityQueryModelVisitorDependencies), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(MongoDbConventionSetBuilderDependencies), new ServiceCharacteristics(ServiceLifetime.Scoped) }
            };

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityFrameworkMongoDbServicesBuilder"/> class.
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/> instance to populate.</param>
        public EntityFrameworkMongoDbServicesBuilder([NotNull] IServiceCollection serviceCollection) : base(serviceCollection)
        {
        }

        /// <summary>
        /// This API supports the Entity Framework Core infrastructure and is not intended to be used directly from your code.
        /// This API may change or be removed in future releases.
        /// </summary>
        protected override ServiceCharacteristics GetServiceCharacteristics(Type serviceType)
            => RelationalServices.TryGetValue(serviceType, out ServiceCharacteristics characteristics)
                ? characteristics
                : base.GetServiceCharacteristics(serviceType);

        /// <summary>
        /// Registers default implementations of all services not already registered by the provider. Database providers must call
        /// this method as the last step of service registration--that is, after all provider services have been registered.
        /// </summary>
        /// <returns>This builder, such that further calls can be chained.</returns>
        public override EntityFrameworkServicesBuilder TryAddCoreServices()
        {
            TryAdd<IDatabaseProvider, DatabaseProvider<MongoDbOptionsExtension>>();
            TryAdd<IModelValidator, MongoDbModelValidator>();
            TryAdd<ITypeMappingSource>(serviceProvider => serviceProvider.GetRequiredService<IMongoDbTypeMappingSource>());
            TryAdd<IMongoDbTypeMappingSource, MongoDbTypeMappingSource>();
            TryAdd<IDatabase, MongoDbDatabase>();
            TryAdd<IDatabaseCreator, MongoDbDatabaseCreator>();
            TryAdd<IValueGeneratorSelector, MongoDbValueGeneratorSelector>();
            TryAdd<IConventionSetBuilder, MongoDbConventionSetBuilder>();
            TryAdd<IQueryContextFactory, MongoDbQueryContextFactory>();
            TryAdd<IEntityQueryableExpressionVisitorFactory, MongoDbEntityQueryableExpressionVisitorFactory>();
            TryAdd<IEntityQueryModelVisitorFactory, MongoDbEntityQueryModelVisitorFactory>();
            TryAdd<IMemberAccessBindingExpressionVisitorFactory, MongoDbMemberAccessBindingExpressionVisitorFactory>();
            TryAdd<INavigationRewritingExpressionVisitorFactory, DocumentNavigationRewritingExpressionVisitorFactory>();

            TryAddProviderSpecificServices(serviceCollectionMap =>
            {
                serviceCollectionMap.TryAddScoped(serviceProvider =>
                {
                    MongoDbOptionsExtension extension = serviceProvider
                        .GetRequiredService<IDbContextOptions>()
                        .FindExtension<MongoDbOptionsExtension>();
                    return extension?.MongoClient ?? new MongoClient();
                });
                serviceCollectionMap.TryAddScoped<IMongoDbConnection, MongoDbConnection>();
                serviceCollectionMap.TryAddScoped<IMongoDbDenormalizedCollectionCompensatingVisitorFactory, MongoDbDenormalizedCollectionCompensatingVisitorFactory>();
                serviceCollectionMap.TryAddScoped<IDocumentQueryExpressionFactory, MongoDbDocumentQueryExpressionFactory>();
                serviceCollectionMap.TryAddScoped<IMongoDbWriteModelFactoryCache, MongoDbWriteModelFactoryCache>();
                serviceCollectionMap.TryAddScoped<IMongoDbWriteModelFactorySelector, MongoDbWriteModelFactorySelector>();
                serviceCollectionMap.TryAddScoped<IEntityLoadInfoFactory, EntityLoadInfoFactory>();
                serviceCollectionMap.TryAddScoped<IValueBufferFactory, ValueBufferFactory>();
            });

            ServiceCollectionMap.GetInfrastructure()
                .AddDependencyScoped<MongoDbConventionSetBuilderDependencies>()
                .AddDependencyScoped<MongoDbEntityQueryModelVisitorDependencies>();

            return base.TryAddCoreServices();
        }
    }
}
