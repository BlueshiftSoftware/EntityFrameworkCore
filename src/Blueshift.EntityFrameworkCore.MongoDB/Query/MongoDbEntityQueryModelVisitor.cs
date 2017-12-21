using Blueshift.EntityFrameworkCore.MongoDB.Query.ExpressionVisitors;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.ResultOperators.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query
{
    /// <inheritdoc />
    public class MongoDbEntityQueryModelVisitor : EntityQueryModelVisitor
    {
        private readonly IQueryOptimizer _queryOptimizer;
        private readonly IQuerySourceTracingExpressionVisitorFactory _querySourceTracingExpressionVisitorFactory;
        private readonly IProjectionExpressionVisitorFactory _projectionExpressionVisitorFactory;
        private readonly FilterApplyingExpressionVisitor _filterApplyingExpressionVisitor;

        private Stack<IList<INavigation>> _includePaths;

        private int _transparentParameterCounter;

        /// <inheritdoc />
        public MongoDbEntityQueryModelVisitor(
            [NotNull] EntityQueryModelVisitorDependencies entityQueryModelVisitorDependencies,
            [NotNull] QueryCompilationContext queryCompilationContext)
            : base(
                Check.NotNull(entityQueryModelVisitorDependencies, nameof(entityQueryModelVisitorDependencies)),
                Check.NotNull(queryCompilationContext, nameof(queryCompilationContext))
            )
        {
            _queryOptimizer = entityQueryModelVisitorDependencies.QueryOptimizer;
            _querySourceTracingExpressionVisitorFactory = entityQueryModelVisitorDependencies
                .QuerySourceTracingExpressionVisitorFactory;
            _projectionExpressionVisitorFactory = entityQueryModelVisitorDependencies
                .ProjectionExpressionVisitorFactory;
            _filterApplyingExpressionVisitor = new FilterApplyingExpressionVisitor(queryCompilationContext);
        }

        private IMongoDbLinqOperatorProvider MongoDbLinqOperatorProvider =>
            (IMongoDbLinqOperatorProvider) LinqOperatorProvider;

        /// <inheritdoc />
        public override void VisitAdditionalFromClause(
            AdditionalFromClause fromClause,
            QueryModel queryModel,
            int index)
        {
            Check.NotNull(fromClause, nameof(fromClause));
            Check.NotNull(queryModel, nameof(queryModel));

            Expression fromExpression = CompileAdditionalFromClauseExpression(fromClause, queryModel);

            ParameterExpression innerItemParameter = Expression.Parameter(
                fromExpression.Type.GetSequenceType(),
                fromClause.ItemName);

            Type transparentIdentifierType = CreateTransparentIdentifierType(CurrentParameter.Type, innerItemParameter.Type);

            Expression memberSelectorExpression = Expression.Lambda(fromExpression, CurrentParameter);

            Expression resultSelectorExpression = Expression.Lambda(
                CallCreateTransparentIdentifier(
                    transparentIdentifierType, CurrentParameter, innerItemParameter),
                CurrentParameter,
                innerItemParameter);

            Expression = Expression.Call(
                LinqOperatorProvider.SelectMany
                    .MakeGenericMethod(
                        CurrentParameter.Type,
                        innerItemParameter.Type,
                        transparentIdentifierType),
                Expression,
                memberSelectorExpression,
                resultSelectorExpression);

            IntroduceTransparentScope(fromClause, queryModel, index, transparentIdentifierType);
        }

        /// <inheritdoc />
        public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, int index)
        {
            Check.NotNull(joinClause, nameof(joinClause));
            Check.NotNull(queryModel, nameof(queryModel));

            LambdaExpression outerKeySelectorExpression = Expression.Lambda(
                ReplaceClauseReferences(joinClause.OuterKeySelector, joinClause),
                CurrentParameter);

            Expression innerSequenceExpression
                = CompileJoinClauseInnerSequenceExpression(joinClause, queryModel);

            ParameterExpression innerItemParameter
                = Expression.Parameter(
                    innerSequenceExpression.Type.GetSequenceType(),
                    joinClause.ItemName);

            QueryCompilationContext.AddOrUpdateMapping(joinClause, innerItemParameter);

            Expression innerKeySelectorExpression = Expression.Lambda(
                ReplaceClauseReferences(joinClause.InnerKeySelector, joinClause),
                innerItemParameter);

            Type transparentIdentifierType = CreateTransparentIdentifierType(CurrentParameter.Type, innerItemParameter.Type);

            Expression resultSelectorExpression = Expression.Lambda(
                CallCreateTransparentIdentifier(
                    transparentIdentifierType,
                    CurrentParameter,
                    innerItemParameter),
                CurrentParameter,
                innerItemParameter);

            Expression = Expression.Call(
                LinqOperatorProvider.Join
                    .MakeGenericMethod(
                        CurrentParameter.Type,
                        innerItemParameter.Type,
                        outerKeySelectorExpression.ReturnType,
                        transparentIdentifierType),
                Expression,
                innerSequenceExpression,
                outerKeySelectorExpression,
                innerKeySelectorExpression,
                resultSelectorExpression);

            IntroduceTransparentScope(joinClause, queryModel, index, transparentIdentifierType);
        }

        /// <inheritdoc />
        public override void VisitGroupJoinClause(GroupJoinClause groupJoinClause, QueryModel queryModel, int index)
        {
            Check.NotNull(groupJoinClause, nameof(groupJoinClause));
            Check.NotNull(queryModel, nameof(queryModel));

            LambdaExpression outerKeySelectorExpression = Expression.Lambda(
                ReplaceClauseReferences(groupJoinClause.JoinClause.OuterKeySelector, groupJoinClause),
                CurrentParameter);

            Expression innerSequenceExpression
                = CompileGroupJoinInnerSequenceExpression(groupJoinClause, queryModel);

            ParameterExpression innerItemParameter = Expression.Parameter(
                innerSequenceExpression.Type.GetSequenceType(),
                groupJoinClause.JoinClause.ItemName);

            QueryCompilationContext.AddOrUpdateMapping(groupJoinClause.JoinClause, innerItemParameter);

            Expression innerKeySelectorExpression = Expression.Lambda(
                ReplaceClauseReferences(groupJoinClause.JoinClause.InnerKeySelector, groupJoinClause),
                innerItemParameter);

            ParameterExpression innerItemsParameter = Expression.Parameter(
                LinqOperatorProvider.MakeSequenceType(innerItemParameter.Type),
                groupJoinClause.ItemName);

            Type transparentIdentifierType = CreateTransparentIdentifierType(CurrentParameter.Type, innerItemsParameter.Type);

            Expression resultSelectorExpression = Expression.Lambda(
                CallCreateTransparentIdentifier(
                    transparentIdentifierType,
                    CurrentParameter,
                    innerItemsParameter),
                CurrentParameter,
                innerItemsParameter);

            Expression = Expression.Call(
                LinqOperatorProvider.GroupJoin
                    .MakeGenericMethod(
                        CurrentParameter.Type,
                        innerItemParameter.Type,
                        outerKeySelectorExpression.ReturnType,
                        transparentIdentifierType),
                Expression,
                innerSequenceExpression,
                outerKeySelectorExpression,
                innerKeySelectorExpression,
                resultSelectorExpression);

            IntroduceTransparentScope(groupJoinClause, queryModel, index, transparentIdentifierType);
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

            if ((selector.Type != Expression.Type.GetSequenceType()
                 || !(selectClause.Selector is QuerySourceReferenceExpression))
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
            if (expression is MemberExpression memberExpression
                && memberExpression.Member.DeclaringType.GetTypeInfo().IsGenericType
                && memberExpression.Member.DeclaringType.GetTypeInfo().GetGenericTypeDefinition() ==
                typeof(MongoDbTransparentIdentifier<,>))
            {
                IList<INavigation> navigations = _includePaths.Pop();
                MethodInfo navigationAssignmentMethodInfo = navigations.Last().IsCollection()
                    ? AssignCollectionMethodInfo
                    : AssignReferenceMethodInfo;
                Type[] genericTypeArguments = memberExpression.Member
                    .DeclaringType
                    .GenericTypeArguments
                    .Select(type => type.TryGetSequenceType() ?? type)
                    .ToArray();
                expression = Expression.Call(
                    null,
                    navigationAssignmentMethodInfo.MakeGenericMethod(genericTypeArguments),
                    ConvertToRelationshipAssignments(memberExpression.Expression),
                    Expression.Constant(navigations));
            }
            return expression;
        }

        /// <inheritdoc />
        protected override void IntroduceTransparentScope(
            IQuerySource querySource,
            QueryModel queryModel,
            int index,
            Type transparentIdentifierType)
        {
            Check.NotNull(querySource, nameof(querySource));
            Check.NotNull(queryModel, nameof(queryModel));
            Check.NotNull(transparentIdentifierType, nameof(transparentIdentifierType));

            CurrentParameter = Expression.Parameter(
                transparentIdentifierType,
                $"t{_transparentParameterCounter++}");

            MemberExpression outerAccessExpression = Expression.Field(
                CurrentParameter,
                transparentIdentifierType,
                OuterPropertyName);

            RescopeTransparentAccess(queryModel.MainFromClause, outerAccessExpression);

            for (var i = 0; i < index; i++)
            {
                if (queryModel.BodyClauses[i] is IQuerySource bodyClause)
                {
                    RescopeTransparentAccess(bodyClause, outerAccessExpression);

                    if (bodyClause is GroupJoinClause groupJoinClause && QueryCompilationContext.QuerySourceMapping
                            .ContainsMapping(groupJoinClause.JoinClause))
                    {
                        RescopeTransparentAccess(groupJoinClause.JoinClause, outerAccessExpression);
                    }
                }
            }

            QueryCompilationContext.AddOrUpdateMapping(
                querySource,
                Expression.Field(
                    CurrentParameter,
                    transparentIdentifierType,
                    InnerPropertyName));
        }

        private void RescopeTransparentAccess(IQuerySource querySource, Expression targetExpression)
            => QueryCompilationContext.QuerySourceMapping.ReplaceMapping(querySource, ShiftMemberAccess(
                targetExpression,
                QueryCompilationContext.QuerySourceMapping.GetExpression(querySource)));

        private static Expression ShiftMemberAccess(Expression targetExpression, Expression currentExpression)
        {
            try
            {
                return currentExpression is MemberExpression memberExpression
                    ? Expression.MakeMemberAccess(
                        ShiftMemberAccess(targetExpression, memberExpression.Expression),
                        memberExpression.Member)
                    : targetExpression;
            }
            catch (ArgumentException)
            {
                // Member is not defined on the new target expression.
                // This is due to stale QuerySourceMappings, which we can't
                // remove due to there not being an API on QuerySourceMapping.
            }

            return currentExpression;
        }

        /// <inheritdoc />
        protected override void OptimizeQueryModel(QueryModel queryModel, bool asyncQuery)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            new MongoDbEagerLoadingExpressionVisitor(QueryCompilationContext, _querySourceTracingExpressionVisitorFactory)
                .VisitQueryModel(queryModel);

            // First pass of optimizations

            _queryOptimizer.Optimize(QueryCompilationContext, queryModel);

            new MongoDbNondeterministicResultCheckingVisitor(QueryCompilationContext.Logger).VisitQueryModel(queryModel);

            // Rewrite includes/navigations

            RewriteProjectedCollectionNavigationsToIncludes(queryModel);

            var includeCompiler = new MongoDbIncludeCompiler(QueryCompilationContext, _querySourceTracingExpressionVisitorFactory);

            includeCompiler.CompileIncludes(queryModel, TrackResults(queryModel), asyncQuery);

            _includePaths = new Stack<IList<INavigation>>(includeCompiler.IncludePaths);

            queryModel.TransformExpressions(new CollectionNavigationSubqueryInjector(this).Visit);
            queryModel.TransformExpressions(new CollectionNavigationSetOperatorSubqueryInjector(this).Visit);

            includeCompiler.RewriteCollectionQueries();

            includeCompiler.LogIgnoredIncludes();

            if (!QueryCompilationContext.IgnoreQueryFilters)
            {
                queryModel.TransformExpressions(_filterApplyingExpressionVisitor.Visit);
            }

            // Second pass of optimizations

            _queryOptimizer.Optimize(QueryCompilationContext, queryModel);

            // Log results

            QueryCompilationContext.Logger.QueryModelOptimized(queryModel);
        }

        private bool TrackResults(QueryModel queryModel)
        {
            // TODO: Unify with QCC

            TrackingResultOperator lastTrackingModifier
                = QueryCompilationContext.QueryAnnotations
                    .OfType<TrackingResultOperator>()
                    .LastOrDefault();

            return !(queryModel.GetOutputDataInfo() is StreamedScalarValueInfo)
                   && (QueryCompilationContext.TrackQueryResults || lastTrackingModifier != null)
                   && (lastTrackingModifier == null
                       || lastTrackingModifier.IsTracking);
        }

        /// <inheritdoc />
        public override void VisitOrdering(
            Ordering ordering,
            QueryModel queryModel,
            OrderByClause orderByClause,
            int index)
        {
            Check.NotNull(ordering, nameof(ordering));
            Check.NotNull(queryModel, nameof(queryModel));
            Check.NotNull(orderByClause, nameof(orderByClause));

            Expression expression = ReplaceClauseReferences(ordering.Expression);

            Expression = Expression.Call(
                (index == 0
                    ? ordering.OrderingDirection == OrderingDirection.Asc
                        ? LinqOperatorProvider.OrderBy
                        : MongoDbLinqOperatorProvider.OrderByDescending
                    : ordering.OrderingDirection == OrderingDirection.Asc
                        ? LinqOperatorProvider.ThenBy
                        : MongoDbLinqOperatorProvider.ThenByDescending)
                .MakeGenericMethod(CurrentParameter.Type, expression.Type),
                Expression,
                Expression.Lambda(expression, CurrentParameter));
        }

        /// <inheritdoc />
        protected override Type CreateTransparentIdentifierType(Type outerType, Type innerType)
            => typeof(MongoDbTransparentIdentifier<,>).MakeGenericType(outerType, innerType);

        /// <inheritdoc />
        protected override Expression CallCreateTransparentIdentifier(
            Type transparentIdentifierType,
            Expression outerExpression,
            Expression innerExpression)
        {
            TypeInfo transparentIdentifierTypeInfo = transparentIdentifierType.GetTypeInfo();
            return Expression.MemberInit(
                Expression.New(transparentIdentifierType),
                Expression.Bind(transparentIdentifierTypeInfo.GetDeclaredField(OuterPropertyName), outerExpression),
                Expression.Bind(transparentIdentifierTypeInfo.GetDeclaredField(InnerPropertyName), innerExpression));
        }

        internal const string OuterPropertyName = nameof(MongoDbTransparentIdentifier<object, object>.Outer);
        internal const string InnerPropertyName = nameof(MongoDbTransparentIdentifier<object, object>.Inner);

        internal class MongoDbTransparentIdentifier<TOuter, TInner>
        {
#pragma warning disable CS0649
            [UsedImplicitly] public TOuter Outer;

            [UsedImplicitly] public TInner Inner;
#pragma warning restore CS0649
        }

        private static readonly MethodInfo AssignReferenceMethodInfo =
            MethodHelper.GetGenericMethodDefinition(() => AssignReference<object, object>(null, null));

        private static readonly MethodInfo AssignCollectionMethodInfo =
            MethodHelper.GetGenericMethodDefinition(() => AssignCollection<object, object>(null, null));

        private static TOuter AssignReference<TOuter, TInner>(
            MongoDbTransparentIdentifier<TOuter, TInner> transparentIdentifier,
            ICollection<INavigation> navigations)
            where TInner : class
        {
            object outer = navigations
                .Take(navigations.Count - 1)
                .Aggregate<IPropertyBase, object>(
                    transparentIdentifier.Outer, 
                    (current, propertyBase) => propertyBase.GetGetter().GetClrValue(current));
            IPropertyBase navigation = navigations.Last();
            navigation.GetSetter().SetClrValue(outer, transparentIdentifier.Inner);
            return transparentIdentifier.Outer;
        }

        private static TOuter AssignCollection<TOuter, TInner>(
            MongoDbTransparentIdentifier<TOuter, IEnumerable<TInner>> transparentIdentifier,
            ICollection<INavigation> navigations)
            where TInner : class
        {
            //object outer = navigations
            //    .Take(navigations.Count - 1)
            //    .Aggregate<IPropertyBase, object>(
            //        transparentIdentifier.Outer,
            //        (current, propertyBase) => propertyBase.GetGetter().GetClrValue(current));
            //INavigation navigation = navigations.Last();

            //IClrCollectionAccessor clrCollectionAccessor = navigation.GetCollectionAccessor();
            //IClrPropertySetter clrPropertySetter = navigation.FindInverse().GetSetter();
            //clrCollectionAccessor.GetOrCreate(outer);

            //foreach (TInner reference in transparentIdentifier.Inner)
            //{
            //    clrCollectionAccessor.Add(outer, reference);
            //    clrPropertySetter.SetClrValue(reference, outer);
            //}

            return transparentIdentifier.Outer;
        }
    }
}
