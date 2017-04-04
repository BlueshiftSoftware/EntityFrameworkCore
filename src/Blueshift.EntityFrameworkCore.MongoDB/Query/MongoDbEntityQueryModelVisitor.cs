using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ResultOperators.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.Logging;
using Remotion.Linq;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class MongoDbEntityQueryModelVisitor : EntityQueryModelVisitor
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public MongoDbEntityQueryModelVisitor(
            [NotNull] EntityQueryModelVisitorDependencies entityQueryModelVisitorDependencies,
            [NotNull] QueryCompilationContext queryCompilationContext)
            : base(
                Check.NotNull(entityQueryModelVisitorDependencies, nameof(entityQueryModelVisitorDependencies)),
                Check.NotNull(queryCompilationContext, nameof(queryCompilationContext))
            )
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static readonly MethodInfo EntityQueryMethodInfo
            = typeof(MongoDbEntityQueryModelVisitor).GetTypeInfo()
                .GetDeclaredMethod(nameof(EntityQuery));

        [UsedImplicitly]
        private static IQueryable<TEntity> EntityQuery<TEntity>(QueryContext queryContext)
            where TEntity : class
            => ((MongoDbQueryContext)queryContext).MongoDbConnection
                .Query<TEntity>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static readonly MethodInfo SubEntityQueryMethodInfo
            = typeof(MongoDbEntityQueryModelVisitor).GetTypeInfo()
                .GetDeclaredMethod(nameof(SubEntityQuery));

        [UsedImplicitly]
        private static IQueryable<TEntity> SubEntityQuery<TBaseEntity, TEntity>(QueryContext queryContext)
            where TBaseEntity : class
            where TEntity : class, TBaseEntity
            => EntityQuery<TBaseEntity>(queryContext)
                .OfType<TEntity>();

        /// <summary>
        ///     Creates an action to asynchronously execute this query.
        /// </summary>
        /// <typeparam name="TResult"> The type of results that the query returns. </typeparam>
        /// <param name="queryModel"> The query. </param>
        /// <returns> An action that asynchronously returns the results of the query. </returns>
        public override Func<QueryContext, IAsyncEnumerable<TResult>> CreateAsyncQueryExecutor<TResult>(
            [NotNull] QueryModel queryModel)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            using (QueryCompilationContext.Logger.BeginScope(this))
            {
                ExtractQueryAnnotations(queryModel);

                var includeResultOperators
                    = QueryCompilationContext.QueryAnnotations
                        .OfType<IncludeResultOperator>()
                        .ToList();

                OptimizeQueryModel(queryModel, includeResultOperators, asyncQuery: true);

                QueryCompilationContext.FindQuerySourcesRequiringMaterialization(this, queryModel);
                QueryCompilationContext.DetermineQueryBufferRequirement(queryModel);

                VisitQueryModel(queryModel);

                Console.WriteLine($"\r\n\r\n==========\r\n\r\nLinqOperaorProvider: {LinqOperatorProvider}\r\n\r\n");

                Console.WriteLine($"Expression.Type Single: {Expression.Type}\r\n\r\n");

                SingleResultToSequence(queryModel, Expression.Type.GetTypeInfo().GenericTypeArguments.FirstOrDefault());

                Console.WriteLine($"Expression.Type ToSequence: {Expression.Type}\r\n\r\n==========\r\n\r\n");

                IncludeNavigations(queryModel, includeResultOperators);

                TrackEntitiesInResults<TResult>(queryModel);

                InterceptExceptions();

                return CreateExecutorLambda<IAsyncEnumerable<TResult>>();
            }
        }
    }
}