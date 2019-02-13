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
    public class EntityFrameworkMongoDbServicesBuilder : EntityFrameworkServicesBuilder
    {
        private static IMongoClient DefaultMongoClientFactory(MongoClientSettings mongoClientSettings)
            => new MongoClient(mongoClientSettings);

        private static readonly IDictionary<Type, ServiceCharacteristics> DocumentServiceCharacteristics
            = new Dictionary<Type, ServiceCharacteristics>
            {
                { typeof(MongoDbEntityQueryModelVisitorDependencies), new ServiceCharacteristics(ServiceLifetime.Scoped) },
                { typeof(MongoDbConventionSetBuilderDependencies), new ServiceCharacteristics(ServiceLifetime.Scoped) }
            };

        /// <inheritdoc />
        public EntityFrameworkMongoDbServicesBuilder(
            [NotNull] IServiceCollection serviceCollection)
            : base(serviceCollection)
        {
        }

        /// <inheritdoc />
        protected override ServiceCharacteristics GetServiceCharacteristics(Type serviceType)
            => DocumentServiceCharacteristics.TryGetValue(serviceType, out ServiceCharacteristics characteristics)
                ? characteristics
                : base.GetServiceCharacteristics(serviceType);

        /// <inheritdoc />
        public override EntityFrameworkServicesBuilder TryAddCoreServices()
        {
            TryAdd<IDatabaseProvider, DatabaseProvider<MongoDbOptionsExtension>>();
            TryAdd<IModelValidator, MongoDbModelValidator>();
            TryAdd<ITypeMappingSource>(serviceProvider => serviceProvider.GetRequiredService<IMongoDbTypeMappingSource>());
            TryAdd<IDatabase, MongoDbDatabase>();
            TryAdd<IDatabaseCreator, MongoDbDatabaseCreator>();
            TryAdd<IValueGeneratorSelector, MongoDbValueGeneratorSelector>();
            TryAdd<IConventionSetBuilder, MongoDbConventionSetBuilder>();
            TryAdd<IQueryContextFactory, MongoDbQueryContextFactory>();
            TryAdd<IQueryCompilationContextFactory, MongoDbQueryCompilationContextFactory>();
            TryAdd<IEntityQueryableExpressionVisitorFactory, MongoDbEntityQueryableExpressionVisitorFactory>();
            TryAdd<IEntityQueryModelVisitorFactory, MongoDbEntityQueryModelVisitorFactory>();
            TryAdd<IMemberAccessBindingExpressionVisitorFactory, MongoDbMemberAccessBindingExpressionVisitorFactory>();
            TryAdd<INavigationRewritingExpressionVisitorFactory, DocumentNavigationRewritingExpressionVisitorFactory>();

            TryAddProviderSpecificServices(serviceCollectionMap =>
            {
                serviceCollectionMap.TryAddScoped(serviceProvider =>
                    serviceProvider
                        .GetRequiredService<IDbContextOptions>()
                        .FindExtension<MongoDbOptionsExtension>()
                        .MongoClientSettings);

                serviceCollectionMap.TryAddScoped<IMongoClientFactory, MongoClientFactory>();
                serviceCollectionMap.TryAddScoped<IMongoDbConnection, MongoDbConnection>();
                serviceCollectionMap.TryAddScoped<IMongoDbDenormalizedCollectionCompensatingVisitorFactory, MongoDbDenormalizedCollectionCompensatingVisitorFactory>();
                serviceCollectionMap.TryAddScoped<IDocumentQueryExpressionFactory, MongoDbDocumentQueryExpressionFactory>();
                serviceCollectionMap.TryAddScoped<IMongoDbWriteModelFactoryCache, MongoDbWriteModelFactoryCache>();
                serviceCollectionMap.TryAddScoped<IMongoDbWriteModelFactorySelector, MongoDbWriteModelFactorySelector>();
                serviceCollectionMap.TryAddScoped<IEntityLoadInfoFactory, EntityLoadInfoFactory>();
                serviceCollectionMap.TryAddScoped<IValueBufferFactory, ValueBufferFactory>();
                serviceCollectionMap.TryAddScoped<IMongoDbTypeMappingSource, MongoDbTypeMappingSource>();
            });

            ServiceCollectionMap
                .GetInfrastructure()
                .AddDependencyScoped<MongoDbConventionSetBuilderDependencies>()
                .AddDependencyScoped<MongoDbEntityQueryModelVisitorDependencies>();

            return base.TryAddCoreServices();
        }
    }
}
