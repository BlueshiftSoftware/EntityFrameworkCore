using Blueshift.EntityFrameworkCore.MongoDB.Query.ExpressionVisitors;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query
{
    /// <summary>
    ///     <para>
    ///         Service dependencies parameter class for <see cref="MongoDbEntityQueryModelVisitor" />
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    ///     <para>
    ///         Do not construct instances of this class directly from either provider or application code as the
    ///         constructor signature may change as new dependencies are added. Instead, use this type in
    ///         your constructor so that an instance will be created and injected automatically by the
    ///         dependency injection container. To create an instance with some dependent services replaced,
    ///         first resolve the object from the dependency injection container, then replace selected
    ///         services using the 'With...' methods. Do not call the constructor at any point in this process.
    ///     </para>
    /// </summary>
    public class MongoDbEntityQueryModelVisitorDependencies
    {
        /// <summary>
        ///     <para>
        ///         Creates the service dependencies parameter object for a <see cref="MongoDbEntityQueryModelVisitorFactory" />.
        ///     </para>
        ///     <para>
        ///         This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///         directly from your code. This API may change or be removed in future releases.
        ///     </para>
        ///     <para>
        ///         Do not call this constructor directly from either provider or application code as it may change
        ///         as new dependencies are added. Instead, use this type in your constructor so that an instance
        ///         will be created and injected automatically by the dependency injection container. To create
        ///         an instance with some dependent services replaced, first resolve the object from the dependency
        ///         injection container, then replace selected services using the 'With...' methods. Do not call
        ///         the constructor at any point in this process.
        ///     </para>
        /// </summary>
        /// <param name="mongoDbDenormalizedCollectionCompensatingVisitorFactory">
        ///     The <see cref="IMongoDbDenormalizedCollectionCompensatingVisitorFactory" /> to be used when processing the query.
        /// </param>
        /// <param name="queryableMethodProvider">
        ///     The <see cref="IQueryableMethodProvider" /> to be used when processing a query.
        /// </param>
        /// <param name="linqProviderFilteringExpressionVisitorFactory">
        ///     The <see cref="ILinqProviderFilteringExpressionVisitorFactory" /> to be used when filtering an expression tree.
        /// </param>
        public MongoDbEntityQueryModelVisitorDependencies(
            [NotNull] IMongoDbDenormalizedCollectionCompensatingVisitorFactory mongoDbDenormalizedCollectionCompensatingVisitorFactory,
            [NotNull] IQueryableMethodProvider queryableMethodProvider,
            [NotNull] ILinqProviderFilteringExpressionVisitorFactory linqProviderFilteringExpressionVisitorFactory)
        {
            MongoDbDenormalizedCollectionCompensatingVisitorFactory
                = Check.NotNull(mongoDbDenormalizedCollectionCompensatingVisitorFactory,
                    nameof(mongoDbDenormalizedCollectionCompensatingVisitorFactory));
            QueryableMethodProvider = Check.NotNull(queryableMethodProvider, nameof(queryableMethodProvider));
            LinqProviderFilteringExpressionVisitorFactory = 
                Check.NotNull(linqProviderFilteringExpressionVisitorFactory,
                    nameof(linqProviderFilteringExpressionVisitorFactory));
        }

        /// <summary>
        ///     Gets the <see cref="IMongoDbDenormalizedCollectionCompensatingVisitorFactory" /> to be used when processing a query.
        /// </summary>
        [NotNull]
        public IMongoDbDenormalizedCollectionCompensatingVisitorFactory
            MongoDbDenormalizedCollectionCompensatingVisitorFactory { get; }

        /// <summary>
        ///     Gets the <see cref="IQueryableMethodProvider" /> to be used when processing a query.
        /// </summary>
        [NotNull]
        public IQueryableMethodProvider QueryableMethodProvider { get; }

        /// <summary>
        ///     Gets the <see cref="ILinqProviderFilteringExpressionVisitorFactory" /> to be used when filtering an expression tree.
        /// </summary>
        [NotNull]
        public ILinqProviderFilteringExpressionVisitorFactory LinqProviderFilteringExpressionVisitorFactory { get; }

    }
}
