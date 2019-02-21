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
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Extensions.DependencyInjection;

namespace Blueshift.EntityFrameworkCore.MongoDB.Infrastructure
{
    /// <inheritdoc />
    public class EntityFrameworkMongoDbServicesBuilder : EntityFrameworkServicesBuilder
    {
        /// <inheritdoc />
        public EntityFrameworkMongoDbServicesBuilder(
            [NotNull] IServiceCollection serviceCollection)
            : base(serviceCollection)
        {
        }

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
            TryAdd<IQueryCompilationContextFactory, QueryableQueryCompilationContextFactory>();
            TryAdd<IQueryContextFactory, MongoDbQueryContextFactory>();
            TryAdd<IEntityQueryableExpressionVisitorFactory, MongoDbEntityQueryableExpressionVisitorFactory>();
            TryAdd<IEntityQueryModelVisitorFactory, MongoDbEntityQueryModelVisitorFactory>();
            TryAdd<IMemberAccessBindingExpressionVisitorFactory, MongoDbMemberAccessBindingExpressionVisitorFactory>();
            TryAdd<INavigationRewritingExpressionVisitorFactory, DocumentNavigationRewritingExpressionVisitorFactory>();
            TryAdd<IQueryCompiler, QueryProviderAdapterQueryCompiler>();
            TryAdd<IResultOperatorHandler, QueryableResultOperatorHandler>();

            TryAddProviderSpecificServices(serviceCollectionMap =>
            {
                serviceCollectionMap.TryAddSingleton<IMongoDbTypeMappingSource, MongoDbTypeMappingSource>();

                serviceCollectionMap.TryAddScoped(serviceProvider =>
                    serviceProvider
                        .GetRequiredService<IDbContextOptions>()
                        .FindExtension<MongoDbOptionsExtension>()
                        .MongoClientSettings);

                serviceCollectionMap.TryAddScoped<IMongoClientFactory, MongoClientFactory>();
                serviceCollectionMap.TryAddScoped<IMongoDbConnection, MongoDbConnection>();
                serviceCollectionMap.TryAddScoped<IQueryableMethodProvider, QueryableMethodProvider>();
                serviceCollectionMap.TryAddScoped<IMongoDbDenormalizedCollectionCompensatingVisitorFactory,
                    MongoDbDenormalizedCollectionCompensatingVisitorFactory>();
                serviceCollectionMap.TryAddScoped<ILinqProviderFilteringExpressionVisitorFactory,
                    LinqProviderFilteringExpressionVisitorFactory>();
                serviceCollectionMap.TryAddScoped<IDocumentQueryExpressionFactory, MongoDbDocumentQueryExpressionFactory>();
                serviceCollectionMap.TryAddScoped<IMongoDbWriteModelFactoryCache, MongoDbWriteModelFactoryCache>();
                serviceCollectionMap.TryAddScoped<IMongoDbWriteModelFactorySelector, MongoDbWriteModelFactorySelector>();
                serviceCollectionMap.TryAddScoped<IEntityLoadInfoFactory, EntityLoadInfoFactory>();
                serviceCollectionMap.TryAddScoped<IValueBufferFactory, ValueBufferFactory>();
            });

            ServiceCollectionMap
                .GetInfrastructure()
                .AddDependencyScoped<MongoDbConventionSetBuilderDependencies>()
                .AddDependencyScoped<MongoDbEntityQueryModelVisitorDependencies>();

            return base.TryAddCoreServices();
        }
    }
}
