using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.ResultOperators.Internal;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query.ExpressionVisitors
{
    internal class MongoDbEagerLoadingExpressionVisitor : QueryModelVisitorBase
    {
        private readonly QueryCompilationContext _queryCompilationContext;
        private readonly QuerySourceTracingExpressionVisitor _querySourceTracingExpressionVisitor;

        public MongoDbEagerLoadingExpressionVisitor(
            QueryCompilationContext queryCompilationContext,
            IQuerySourceTracingExpressionVisitorFactory querySourceTracingExpressionVisitorFactory)
        {
            _queryCompilationContext = queryCompilationContext;

            _querySourceTracingExpressionVisitor = querySourceTracingExpressionVisitorFactory.Create();
        }

        public override void VisitMainFromClause(MainFromClause fromClause, QueryModel queryModel)
        {
            ApplyIncludesForOwnedNavigations(new QuerySourceReferenceExpression(fromClause), queryModel);

            base.VisitMainFromClause(fromClause, queryModel);
        }

        protected override void VisitBodyClauses(ObservableCollection<IBodyClause> bodyClauses, QueryModel queryModel)
        {
            foreach (var querySource in bodyClauses.OfType<IQuerySource>())
            {
                ApplyIncludesForOwnedNavigations(new QuerySourceReferenceExpression(querySource), queryModel);
            }

            base.VisitBodyClauses(bodyClauses, queryModel);
        }

        private void ApplyIncludesForOwnedNavigations(QuerySourceReferenceExpression querySourceReferenceExpression,
            QueryModel queryModel)
        {
            if (_querySourceTracingExpressionVisitor
                    .FindResultQuerySourceReferenceExpression(
                        queryModel.SelectClause.Selector,
                        querySourceReferenceExpression.ReferencedQuerySource) != null)
            {
                var entityType = _queryCompilationContext.Model.FindEntityType(querySourceReferenceExpression.Type);

                if (entityType != null)
                {
                    var stack = new Stack<INavigation>();

                    WalkNavigations(querySourceReferenceExpression, entityType, stack);
                }
            }
        }

        private void WalkNavigations(Expression querySourceReferenceExpression, IEntityType entityType,
            Stack<INavigation> stack)
        {
            var outboundNavigations
                = entityType.GetNavigations()
                    .Concat(entityType.GetDerivedTypes().SelectMany(et => et.GetDeclaredNavigations()))
                    .Where(n => n.IsEagerLoaded)
                    .ToList();

            if (outboundNavigations.Count == 0
                && stack.Count > 0)
            {
                _queryCompilationContext.AddAnnotations(
                    new[]
                    {
                        new IncludeResultOperator(
                            stack.Reverse().ToArray(),
                            querySourceReferenceExpression,
                            implicitLoad: true)
                    });
            }
            else
            {
                foreach (var navigation in outboundNavigations)
                {
                    stack.Push(navigation);

                    WalkNavigations(querySourceReferenceExpression, navigation.GetTargetType(), stack);

                    stack.Pop();
                }
            }
        }
    }
}
