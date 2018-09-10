using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Blueshift.EntityFrameworkCore.MongoDB.Query.ExpressionVisitors;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query
{
    /// <inheritdoc />
    public class MongoDbEntityQueryModelVisitor : EntityQueryModelVisitor
    {
        private readonly IProjectionExpressionVisitorFactory _projectionExpressionVisitorFactory;
        private readonly IMongoDbDenormalizedCollectionCompensatingVisitorFactory
            _mongoDbDenormalizedCollectionCompensatingVisitorFactory;

        /// <inheritdoc />
        public MongoDbEntityQueryModelVisitor(
            [NotNull] EntityQueryModelVisitorDependencies entityQueryModelVisitorDependencies,
            [NotNull] QueryCompilationContext queryCompilationContext,
            [NotNull] MongoDbEntityQueryModelVisitorDependencies mongoDbEntityQueryModelVisitorDependencies)
            : base(
                Check.NotNull(entityQueryModelVisitorDependencies, nameof(entityQueryModelVisitorDependencies)),
                Check.NotNull(queryCompilationContext, nameof(queryCompilationContext))
            )
        {
            _projectionExpressionVisitorFactory = entityQueryModelVisitorDependencies
                .ProjectionExpressionVisitorFactory;
            _mongoDbDenormalizedCollectionCompensatingVisitorFactory
                = Check.NotNull(mongoDbEntityQueryModelVisitorDependencies, nameof(mongoDbEntityQueryModelVisitorDependencies))
                    .MongoDbDenormalizedCollectionCompensatingVisitorFactory;
        }

        /// <inheritdoc />
        public override void VisitAdditionalFromClause(AdditionalFromClause fromClause, QueryModel queryModel, int index)
        {
            base.VisitAdditionalFromClause(fromClause, queryModel, index);
        }

        /// <inheritdoc />
        public override void VisitMainFromClause(MainFromClause fromClause, QueryModel queryModel)
        {
            base.VisitMainFromClause(fromClause, queryModel);
        }

        /// <inheritdoc />
        public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, GroupJoinClause groupJoinClause)
        {
            base.VisitJoinClause(joinClause, queryModel, groupJoinClause);
        }

        /// <inheritdoc />
        public override void VisitGroupJoinClause(GroupJoinClause groupJoinClause, QueryModel queryModel, int index)
        {
            base.VisitGroupJoinClause(groupJoinClause, queryModel, index);
        }

        /// <inheritdoc />
        public override void VisitSelectClause(
            SelectClause selectClause,
            QueryModel queryModel)
        {
            Check.NotNull(selectClause, nameof(selectClause));
            Check.NotNull(queryModel, nameof(queryModel));

            if (selectClause.Selector.Type == Expression.Type.GetSequenceType()
                && selectClause.Selector is QuerySourceReferenceExpression)
            {
                return;
            }

            Expression selector = ReplaceClauseReferences(
                _projectionExpressionVisitorFactory
                    .Create(this, queryModel.MainFromClause)
                    .Visit(selectClause.Selector),
                inProjection: true);

            if ((Expression.Type.TryGetSequenceType() != null || !(selectClause.Selector is QuerySourceReferenceExpression))
                && !queryModel.ResultOperators
                    .Select(ro => ro.GetType())
                    .Any(
                        t => t == typeof(GroupResultOperator)
                             || t == typeof(AllResultOperator)))
            {
                Expression = Expression.Call(
                    LinqOperatorProvider.Select
                        .MakeGenericMethod(CurrentParameter.Type, selector.Type),
                    Expression,
                    Expression.Lambda(ConvertToRelationshipAssignments(selector), CurrentParameter));
            }
        }

        private Expression ConvertToRelationshipAssignments(Expression expression)
        {
            if (expression is MethodCallExpression methodCallExpression
                && IncludeCompiler.IsIncludeMethod(methodCallExpression))
            {
                expression = (MethodCallExpression) _mongoDbDenormalizedCollectionCompensatingVisitorFactory
                    .Create()
                    .Visit(methodCallExpression);
            }
            return expression;
        }
    }
}
