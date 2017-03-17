using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Blueshift.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Blueshift.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Blueshift.EntityFrameworkCore.ValueGeneration;
using Microsoft.EntityFrameworkCore.Query;
using Blueshift.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Blueshift.EntityFrameworkCore.Metadata.Builders;
using MongoDB.Driver;

namespace Blueshift.EntityFrameworkCore.MongoDB.Infrastructure
{
    public class EntityFrameworkMongoDbServicesBuilder : EntityFrameworkServicesBuilder
    {
        private static readonly IDictionary<Type, ServiceCharacteristics> _relationalServices
            = new Dictionary<Type, ServiceCharacteristics>
            {
                { typeof(IMongoClient), new ServiceCharacteristics(ServiceLifetime.Singleton) },
                { typeof(IMongoDbConnection), new ServiceCharacteristics(ServiceLifetime.Scoped) }
            };

        public EntityFrameworkMongoDbServicesBuilder([NotNull] IServiceCollection serviceCollection) : base(serviceCollection)
        {
        }

        protected override ServiceCharacteristics GetServiceCharacteristics(Type serviceType)
            => _relationalServices.TryGetValue(serviceType, out ServiceCharacteristics characteristics)
                ? characteristics
                : base.GetServiceCharacteristics(serviceType);

        public override EntityFrameworkServicesBuilder TryAddCoreServices()
        {
            TryAdd<IDatabaseProvider, DatabaseProvider<MongoDbOptionsExtension>>();
            TryAdd<IModelSource, MongoDbModelSource>();
            TryAdd<IModelValidator, MongoDbModelValidator>();
            TryAdd<IDatabase, MongoDbDatabase>();
            TryAdd<IDatabaseCreator, MongoDbDatabaseCreator>();
            TryAdd<IValueGeneratorSelector, MongoDbValueGeneratorSelector>();
            TryAdd<IConventionSetBuilder, MongoDbConventionSetBuilder>();
            TryAdd<IQueryContextFactory, MongoDbQueryContextFactory>();
            TryAdd<IEntityQueryableExpressionVisitorFactory, MongoDbEntityQueryableExpressionVisitorFactory>();
            TryAdd<IEntityQueryModelVisitorFactory, MongoDbEntityQueryModelVisitorFactory>();
            TryAdd<IValueGeneratorCache, MongoDbValueGeneratorCache>();
            TryAddProviderSpecificServices(serviceCollectionMap =>
            {
                serviceCollectionMap.TryAddScoped<IMongoDbConnection, MongoDbConnection>();
                serviceCollectionMap.TryAddScoped(serviceProvider =>
                {
                    var extension = serviceProvider.GetRequiredService<IDbContextOptions>().FindExtension<MongoDbOptionsExtension>();
                    IMongoClient mongoClient;
                    if (extension?.MongoClient != null)
                    {
                        mongoClient = extension.MongoClient;
                    }
                    else if (extension?.MongoUrl != null)
                    {
                        mongoClient = new MongoClient(extension.MongoUrl);
                    }
                    else if (extension?.MongoClientSettings != null)
                    {
                        mongoClient = new MongoClient(extension.MongoClientSettings);
                    }
                    else if (!string.IsNullOrWhiteSpace(extension.ConnectionString))
                    {
                        mongoClient = new MongoClient(extension.ConnectionString);
                    }
                    else
                    {
                        mongoClient = new MongoClient();
                    }
                    return mongoClient;
                });
            });

            return base.TryAddCoreServices();
        }
    }
}
