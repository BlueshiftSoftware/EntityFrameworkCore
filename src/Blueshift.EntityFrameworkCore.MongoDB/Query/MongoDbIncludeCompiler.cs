using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.ResultOperators.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query
{
    /// <inheritdoc />
    public class MongoDbIncludeCompiler : IncludeCompiler
    {
        private readonly QueryCompilationContext _queryCompilationContext;
        private readonly IQuerySourceTracingExpressionVisitorFactory _querySourceTracingExpressionVisitorFactory;
        private readonly List<IncludeResultOperator> _includeResultOperators;

        /// <inheritdoc />
        public MongoDbIncludeCompiler(
            [NotNull] QueryCompilationContext queryCompilationContext,
            [NotNull] IQuerySourceTracingExpressionVisitorFactory querySourceTracingExpressionVisitorFactory)
            : base(
                  Check.NotNull(queryCompilationContext, nameof(queryCompilationContext)),
                  Check.NotNull(querySourceTracingExpressionVisitorFactory, nameof(querySourceTracingExpressionVisitorFactory)))
        {
            _queryCompilationContext = queryCompilationContext;
            _querySourceTracingExpressionVisitorFactory = querySourceTracingExpressionVisitorFactory;

            _includeResultOperators
                = _queryCompilationContext.QueryAnnotations
                    .OfType<IncludeResultOperator>()
                    .ToList();
        }

        /// <summary>
        /// Gets a <see cref="Stack{T}"/> of <see cref="IList{T}"/> of <see cref="INavigation"/> properties representing the processed includes.
        /// </summary>
        public Stack<IList<INavigation>> IncludePaths { get; } = new Stack<IList<INavigation>>();

        /// <inheritdoc />
        public override void CompileIncludes(QueryModel queryModel, bool trackingQuery, bool asyncQuery)
        {
            IDictionary<QuerySourceReferenceExpression, IncludeResultOperator> includedQuerySources
                = GetIncludedQuerySourceReferenceExpressions(queryModel);

            Expression selectExpression = queryModel.SelectClause.Selector;
            foreach (KeyValuePair<QuerySourceReferenceExpression, IncludeResultOperator> kvp in includedQuerySources)
            {
                INavigation navigation = null;
                IList<INavigation> navigations = kvp.Value
                    .NavigationPropertyPaths
                    .Select(memberName => navigation = _queryCompilationContext.Model
                        .FindEntityType(navigation?.GetTargetType().ClrType ?? kvp.Key.Type)
                        .FindNavigation(memberName))
                    .ToList();

                IncludePaths.Push(navigations);

                IBodyClause includedNavigationClause = navigation.IsCollection()
                    ? CompileCollectionInclude(navigations, kvp.Key)
                    : CompileReferenceInclude(navigations, kvp.Key);

                queryModel.BodyClauses.Add(includedNavigationClause);
            }

            queryModel.SelectClause.Selector = selectExpression;
        }

        private IBodyClause CompileCollectionInclude(
            ICollection<INavigation> navigations,
            Expression rootExpression)
        {
            Expression navigationExpression = navigations
                .Take(navigations.Count - 1)
                .Aggregate(
                    rootExpression,
                    (expression, memberNavigation) => Expression.MakeMemberAccess(
                        expression,
                        memberNavigation.GetMemberInfo(false, false)));

            INavigation navigation = navigations.Last();
            IEntityType innerEntityType = navigation.GetTargetType();
            INavigation inverseNavigation = navigation.FindInverse();
            IProperty outerKeyProperty = inverseNavigation.ForeignKey.PrincipalKey.Properties.First();

            var includeJoinClause = new JoinClause(
                MongoDbUtilities.ToLowerCamelCase(innerEntityType.ClrType.Name),
                innerEntityType.ClrType,
                NullAsyncQueryProvider.Instance.CreateEntityQueryableExpression(innerEntityType.ClrType),
                Expression.MakeMemberAccess(
                    navigationExpression,
                    outerKeyProperty.GetMemberInfo(false, false)),
                Expression.Constant(null));

            var innerItemQuerySourceReferenceExpression = new QuerySourceReferenceExpression(includeJoinClause);
            _queryCompilationContext.AddOrUpdateMapping(includeJoinClause, innerEntityType);

            includeJoinClause.InnerKeySelector = Expression.MakeMemberAccess(
                Expression.MakeMemberAccess(
                    innerItemQuerySourceReferenceExpression,
                    inverseNavigation.GetMemberInfo(false, false)),
                outerKeyProperty.GetMemberInfo(false, false));

            return new GroupJoinClause(
                includeJoinClause.ItemName,
                typeof(IEnumerable<>).MakeGenericType(includeJoinClause.ItemType),
                includeJoinClause);
        }

        private IBodyClause CompileReferenceInclude(
            IList<INavigation> navigations,
            Expression rootExpression)
        {
            Expression navigationExpression = navigations
                .Aggregate(
                    rootExpression,
                    (expression, memberNavigation) => Expression.MakeMemberAccess(
                        expression,
                        memberNavigation.GetMemberInfo(false, false)));

            INavigation navigation = navigations.Last();
            IEntityType innerEntityType = navigation.GetTargetType();
            IProperty innerKeyProperty = navigation.ForeignKey.PrincipalKey.Properties.First();

            var includeJoinClause = new JoinClause(
                MongoDbUtilities.ToLowerCamelCase(innerEntityType.ClrType.Name),
                innerEntityType.ClrType,
                NullAsyncQueryProvider.Instance.CreateEntityQueryableExpression(innerEntityType.ClrType),
                Expression.MakeMemberAccess(
                    navigationExpression,
                    innerKeyProperty.GetMemberInfo(false, false)),
                Expression.Constant(null));

            var innerItemQuerySourceReferenceExpression = new QuerySourceReferenceExpression(includeJoinClause);
            _queryCompilationContext.AddOrUpdateMapping(includeJoinClause, innerEntityType);

            includeJoinClause.InnerKeySelector = Expression.MakeMemberAccess(
                innerItemQuerySourceReferenceExpression,
                innerKeyProperty.GetMemberInfo(false, false));

            return includeJoinClause;
        }

        private IDictionary<QuerySourceReferenceExpression, IncludeResultOperator>
            GetIncludedQuerySourceReferenceExpressions(QueryModel queryModel)
        {
            var includedQuerySources = new ConcurrentDictionary<QuerySourceReferenceExpression, IncludeResultOperator>();

            QuerySourceTracingExpressionVisitor querySourceTracingExpressionVisitor
                = _querySourceTracingExpressionVisitorFactory.Create();

            foreach (IncludeResultOperator includeResultOperator in _includeResultOperators.ToArray())
            {
                QuerySourceReferenceExpression querySourceReferenceExpression
                    = querySourceTracingExpressionVisitor
                        .FindResultQuerySourceReferenceExpression(
                            queryModel.GetOutputExpression(),
                            includeResultOperator.QuerySource);

                if (querySourceReferenceExpression == null)
                {
                    continue;
                }

                if (querySourceReferenceExpression.Type.IsGrouping()
                    && querySourceTracingExpressionVisitor.OriginGroupByQueryModel != null)
                {
                    querySourceReferenceExpression
                        = querySourceTracingExpressionVisitor
                            .FindResultQuerySourceReferenceExpression(
                                querySourceTracingExpressionVisitor.OriginGroupByQueryModel.GetOutputExpression(),
                                includeResultOperator.QuerySource);
                }

                if (querySourceReferenceExpression == null || querySourceReferenceExpression.Type.IsGrouping())
                {
                    continue;
                }

                includedQuerySources.GetOrAdd(querySourceReferenceExpression, qsre => includeResultOperator);

                _queryCompilationContext.Logger.NavigationIncluded(includeResultOperator);
                _includeResultOperators.Remove(includeResultOperator);
            }

            return includedQuerySources;
        }
    }
}
