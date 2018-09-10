using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.ResultOperators.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ExpressionVisitors;
using Remotion.Linq.Clauses.ResultOperators;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query.ExpressionVisitors
{
    /// <inheritdoc />
    public class MongoDbNavigationRewritingExpressionVisitor : NavigationRewritingExpressionVisitor
    {
        private readonly EntityQueryModelVisitor _queryModelVisitor;
        private readonly NavigationJoins _navigationJoins = new NavigationJoins();
        private readonly MongoDbNavigationRewritingQueryModelVisitor _mongoDbNavigationRewritingQueryModelVisitor;
        private QueryModel _queryModel;
        private QueryModel _parentQueryModel;

        private bool _insideInnerKeySelector;
        private bool _insideOrderBy;
        private bool _insideMaterializeCollectionNavigation;

        private class NavigationJoins : IEnumerable<NavigationJoin>
        {
            private readonly Dictionary<NavigationJoin, int> _navigationJoins = new Dictionary<NavigationJoin, int>();

            public void Add(NavigationJoin navigationJoin)
            {
                _navigationJoins.TryGetValue(navigationJoin, out int count);
                _navigationJoins[navigationJoin] = ++count;
            }

            public bool Remove(NavigationJoin navigationJoin)
            {
                if (_navigationJoins.TryGetValue(navigationJoin, out int count))
                {
                    if (count > 1)
                    {
                        _navigationJoins[navigationJoin] = --count;
                    }
                    else
                    {
                        _navigationJoins.Remove(navigationJoin);
                    }

                    return true;
                }

                return false;
            }

            public IEnumerator<NavigationJoin> GetEnumerator() => _navigationJoins.Keys.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => _navigationJoins.Keys.GetEnumerator();
        }

        private class NavigationJoin
        {
            public static void RemoveNavigationJoin(
                NavigationJoins navigationJoins, NavigationJoin navigationJoin)
            {
                if (!navigationJoins.Remove(navigationJoin))
                {
                    foreach (NavigationJoin nj in navigationJoins)
                    {
                        nj.Children.Remove(navigationJoin);
                    }
                }
            }

            public NavigationJoin(
                IQuerySource querySource,
                INavigation navigation,
                JoinClause joinClause,
                IEnumerable<IBodyClause> additionalBodyClauses,
                bool dependentToPrincipal,
                QuerySourceReferenceExpression querySourceReferenceExpression)
                : this(
                    querySource,
                    navigation,
                    joinClause,
                    null,
                    additionalBodyClauses,
                    dependentToPrincipal,
                    querySourceReferenceExpression)
            {
            }

            public NavigationJoin(
                IQuerySource querySource,
                INavigation navigation,
                GroupJoinClause groupJoinClause,
                IEnumerable<IBodyClause> additionalBodyClauses,
                bool dependentToPrincipal,
                QuerySourceReferenceExpression querySourceReferenceExpression)
                : this(
                    querySource,
                    navigation,
                    null,
                    groupJoinClause,
                    additionalBodyClauses,
                    dependentToPrincipal,
                    querySourceReferenceExpression)
            {
            }

            private NavigationJoin(
                IQuerySource querySource,
                INavigation navigation,
                JoinClause joinClause,
                GroupJoinClause groupJoinClause,
                IEnumerable<IBodyClause> additionalBodyClauses,
                bool dependentToPrincipal,
                QuerySourceReferenceExpression querySourceReferenceExpression)
            {
                QuerySource = querySource;
                Navigation = navigation;
                JoinClause = joinClause;
                GroupJoinClause = groupJoinClause;
                AdditionalBodyClauses = additionalBodyClauses;
                DependentToPrincipal = dependentToPrincipal;
                QuerySourceReferenceExpression = querySourceReferenceExpression;
            }

            public IQuerySource QuerySource { get; }
            public INavigation Navigation { get; }
            public JoinClause JoinClause { get; }
            public GroupJoinClause GroupJoinClause { get; }
            public bool DependentToPrincipal { get; }
            public QuerySourceReferenceExpression QuerySourceReferenceExpression { get; }
            public readonly NavigationJoins Children = new NavigationJoins();

            private IEnumerable<IBodyClause> AdditionalBodyClauses { get; }

            private bool IsInserted { get; set; }

            public IEnumerable<NavigationJoin> Iterate()
            {
                yield return this;

                foreach (NavigationJoin navigationJoin in Children.SelectMany(nj => nj.Iterate()))
                {
                    yield return navigationJoin;
                }
            }

            public void Insert(QueryModel queryModel)
            {
                int insertionIndex = 0;

                if (QuerySource is IBodyClause bodyClause)
                {
                    insertionIndex = queryModel.BodyClauses.IndexOf(bodyClause) + 1;
                }

                if (queryModel.MainFromClause == QuerySource
                    || insertionIndex > 0)
                {
                    foreach (NavigationJoin nj in Iterate())
                    {
                        nj.Insert(queryModel, ref insertionIndex);
                    }
                }
            }

            private void Insert(QueryModel queryModel, ref int insertionIndex)
            {
                if (IsInserted)
                {
                    return;
                }

                queryModel.BodyClauses.Insert(insertionIndex++, JoinClause ?? (IBodyClause)GroupJoinClause);

                foreach (IBodyClause additionalBodyClause in AdditionalBodyClauses)
                {
                    queryModel.BodyClauses.Insert(insertionIndex++, additionalBodyClause);
                }

                IsInserted = true;
            }
        }

        /// <inheritdoc />
        public MongoDbNavigationRewritingExpressionVisitor([NotNull] EntityQueryModelVisitor queryModelVisitor)
            : this(queryModelVisitor, navigationExpansionSubquery: false)
        {
        }

        /// <inheritdoc />
        public MongoDbNavigationRewritingExpressionVisitor(
            [NotNull] EntityQueryModelVisitor queryModelVisitor,
            bool navigationExpansionSubquery)
            : base(
                Check.NotNull(queryModelVisitor, nameof(queryModelVisitor)),
                navigationExpansionSubquery)
        {
            Check.NotNull(queryModelVisitor, nameof(queryModelVisitor));

            _queryModelVisitor = queryModelVisitor;
            _mongoDbNavigationRewritingQueryModelVisitor = new MongoDbNavigationRewritingQueryModelVisitor(this, _queryModelVisitor, navigationExpansionSubquery);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual bool RewriteOwnedNavigations { get; set; } = false;

        /// <inheritdoc />
        public override void Rewrite(QueryModel queryModel, QueryModel parentQueryModel)
        {
            _queryModel = queryModel;
            _parentQueryModel = parentQueryModel;

            _mongoDbNavigationRewritingQueryModelVisitor.VisitQueryModel(_queryModel);

            foreach (NavigationJoin navigationJoin in _navigationJoins)
            {
                navigationJoin.Insert(_queryModel);
            }

            if (parentQueryModel != null)
            {
                _queryModel = parentQueryModel;
            }
        }

        /// <inheritdoc />
        protected override Expression VisitUnary(UnaryExpression node)
        {
            Expression newOperand = Visit(node.Operand);

            return node.NodeType == ExpressionType.Convert && newOperand.Type == node.Type
                ? newOperand
                : node.Update(newOperand);
        }

        /// <inheritdoc />
        protected override Expression VisitSubQuery(SubQueryExpression expression)
        {
            bool oldInsideInnerKeySelector = _insideInnerKeySelector;
            _insideInnerKeySelector = false;

            Rewrite(expression.QueryModel, _queryModel);

            _insideInnerKeySelector = oldInsideInnerKeySelector;

            return expression;
        }

        /// <inheritdoc />
        protected override Expression VisitBinary(BinaryExpression node)
        {
            Expression newLeft = Visit(node.Left);
            Expression newRight = Visit(node.Right);

            if (newLeft == node.Left
                && newRight == node.Right)
            {
                return node;
            }

            NavigationJoin leftNavigationJoin
                = _navigationJoins
                    .SelectMany(nj => nj.Iterate())
                    .FirstOrDefault(nj => ReferenceEquals(nj.QuerySourceReferenceExpression, newLeft));

            NavigationJoin rightNavigationJoin
                = _navigationJoins
                    .SelectMany(nj => nj.Iterate())
                    .FirstOrDefault(nj => ReferenceEquals(nj.QuerySourceReferenceExpression, newRight));

            JoinClause leftJoin = leftNavigationJoin?.JoinClause ?? leftNavigationJoin?.GroupJoinClause?.JoinClause;
            JoinClause rightJoin = rightNavigationJoin?.JoinClause ?? rightNavigationJoin?.GroupJoinClause?.JoinClause;

            if (leftNavigationJoin != null
                && !leftNavigationJoin.Navigation.GetTargetType().IsOwned())
            {
                if (newRight.IsNullConstantExpression())
                {
                    if (leftNavigationJoin.DependentToPrincipal)
                    {
                        newLeft = leftJoin?.OuterKeySelector;

                        NavigationJoin.RemoveNavigationJoin(_navigationJoins, leftNavigationJoin);

                        if (newLeft != null
                            && IsCompositeKey(newLeft.Type))
                        {
                            newRight = CreateNullCompositeKey(newLeft);
                        }
                    }
                }
                else
                {
                    newLeft = leftJoin?.InnerKeySelector;
                }
            }

            if (rightNavigationJoin != null
                && !rightNavigationJoin.Navigation.GetTargetType().IsOwned())
            {
                if (newLeft.IsNullConstantExpression())
                {
                    if (rightNavigationJoin.DependentToPrincipal)
                    {
                        newRight = rightJoin?.OuterKeySelector;

                        NavigationJoin.RemoveNavigationJoin(_navigationJoins, rightNavigationJoin);

                        if (newRight != null
                            && IsCompositeKey(newRight.Type))
                        {
                            newLeft = CreateNullCompositeKey(newRight);
                        }
                    }
                }
                else
                {
                    newRight = rightJoin?.InnerKeySelector;
                }
            }

            if (node.NodeType != ExpressionType.ArrayIndex
                && node.NodeType != ExpressionType.Coalesce
                && newLeft != null
                && newRight != null
                && newLeft.Type != newRight.Type)
            {
                if (newLeft.Type.IsNullableType()
                    && !newRight.Type.IsNullableType())
                {
                    newRight = Expression.Convert(newRight, newLeft.Type);
                }
                else if (!newLeft.Type.IsNullableType()
                         && newRight.Type.IsNullableType())
                {
                    newLeft = Expression.Convert(newLeft, newRight.Type);
                }
            }

            return Expression.MakeBinary(node.NodeType, newLeft, newRight, node.IsLiftedToNull, node.Method);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitConditional(ConditionalExpression node)
        {
            Expression test = Visit(node.Test);
            if (test.Type == typeof(bool?))
            {
                test = Expression.Equal(test, Expression.Constant(true, typeof(bool?)));
            }

            Expression ifTrue = Visit(node.IfTrue);
            Expression ifFalse = Visit(node.IfFalse);

            if (ifTrue.Type.IsNullableType()
                && !ifFalse.Type.IsNullableType())
            {
                ifFalse = Expression.Convert(ifFalse, ifTrue.Type);
            }

            if (ifFalse.Type.IsNullableType()
                && !ifTrue.Type.IsNullableType())
            {
                ifTrue = Expression.Convert(ifTrue, ifFalse.Type);
            }

            return test != node.Test || ifTrue != node.IfTrue || ifFalse != node.IfFalse
                ? Expression.Condition(test, ifTrue, ifFalse)
                : node;
        }

        private static NewExpression CreateNullCompositeKey(Expression otherExpression)
            => Expression.New(
                AnonymousObject.AnonymousObjectCtor,
                Expression.NewArrayInit(
                    typeof(object),
                    Enumerable.Repeat(
                        Expression.Constant(null),
                        ((NewArrayExpression)((NewExpression)otherExpression).Arguments.Single()).Expressions.Count)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitMember(MemberExpression node)
        {
            Check.NotNull(node, nameof(node));

            Expression result = _queryModelVisitor.BindNavigationPathPropertyExpression(
                node,
                (ps, qs) =>
                {
                    return qs != null
                        ? RewriteNavigationProperties(
                            ps,
                            qs,
                            node,
                            node.Expression,
                            node.Member.Name,
                            node.Type,
                            e => e.MakeMemberAccess(node.Member),
                            e => new NullConditionalExpression(e, e.MakeMemberAccess(node.Member)))
                        : null;
                });

            if (result != null)
            {
                return result;
            }

            Expression newExpression = Visit(node.Expression);

            MemberExpression newMemberExpression = newExpression != node.Expression
                ? newExpression.MakeMemberAccess(node.Member)
                : node;

            result = NeedsNullCompensation(newExpression)
                ? (Expression)new NullConditionalExpression(
                    newExpression,
                    newMemberExpression)
                : newMemberExpression;

            return result.Type == typeof(bool?) && node.Type == typeof(bool)
                ? Expression.Equal(result, Expression.Constant(true, typeof(bool?)))
                : result;
        }

        private readonly Dictionary<QuerySourceReferenceExpression, bool> _nullCompensationNecessityMap
            = new Dictionary<QuerySourceReferenceExpression, bool>();

        private bool NeedsNullCompensation(Expression expression)
        {
            if (expression is QuerySourceReferenceExpression qsre)
            {
                if (_nullCompensationNecessityMap.TryGetValue(qsre, out bool result))
                {
                    return result;
                }

                SubQueryExpression subQuery = (qsre.ReferencedQuerySource as FromClauseBase)?.FromExpression as SubQueryExpression
                               ?? (qsre.ReferencedQuerySource as JoinClause)?.InnerSequence as SubQueryExpression;

                // if qsre is pointing to a subquery, look for DefaulIfEmpty result operators inside
                // if such operator is found then we need to add null-compensation logic
                // unless the query model has a GroupBy operator - qsre coming from groupby can never be null
                if (subQuery != null
                    && !(subQuery.QueryModel.ResultOperators.LastOrDefault() is GroupResultOperator))
                {
                    ContainsDefaultIfEmptyCheckingVisitor containsDefaultIfEmptyChecker = new ContainsDefaultIfEmptyCheckingVisitor();
                    containsDefaultIfEmptyChecker.VisitQueryModel(subQuery.QueryModel);
                    if (!containsDefaultIfEmptyChecker.ContainsDefaultIfEmpty)
                    {
                        subQuery.QueryModel.TransformExpressions(
                            e => new TransformingQueryModelExpressionVisitor<ContainsDefaultIfEmptyCheckingVisitor>(containsDefaultIfEmptyChecker).Visit(e));
                    }

                    _nullCompensationNecessityMap[qsre] = containsDefaultIfEmptyChecker.ContainsDefaultIfEmpty;

                    return containsDefaultIfEmptyChecker.ContainsDefaultIfEmpty;
                }

                _nullCompensationNecessityMap[qsre] = false;
            }

            return false;
        }

        private class ContainsDefaultIfEmptyCheckingVisitor : QueryModelVisitorBase
        {
            public bool ContainsDefaultIfEmpty { get; private set; }

            public override void VisitResultOperator(ResultOperatorBase resultOperator, QueryModel queryModel, int index)
            {
                if (resultOperator is DefaultIfEmptyResultOperator)
                {
                    ContainsDefaultIfEmpty = true;
                }
            }
        }

        /// <inheritdoc />
        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            Expression newExpression = CompensateForNullabilityDifference(
                Visit(node.Expression),
                node.Expression.Type);

            return node.Update(newExpression);
        }

        /// <inheritdoc />
        protected override ElementInit VisitElementInit(ElementInit node)
        {
            List<Type> originalArgumentTypes = node.Arguments.Select(a => a.Type).ToList();
            List<Expression> newArguments = VisitAndConvert(node.Arguments, nameof(VisitElementInit)).ToList();

            for (int i = 0; i < newArguments.Count; i++)
            {
                newArguments[i] = CompensateForNullabilityDifference(newArguments[i], originalArgumentTypes[i]);
            }

            return node.Update(newArguments);
        }

        /// <inheritdoc />
        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            List<Type> originalExpressionTypes = node.Expressions.Select(e => e.Type).ToList();
            List<Expression> newExpressions = VisitAndConvert(node.Expressions, nameof(VisitNewArray)).ToList();

            for (int i = 0; i < newExpressions.Count; i++)
            {
                newExpressions[i] = CompensateForNullabilityDifference(newExpressions[i], originalExpressionTypes[i]);
            }

            return node.Update(newExpressions);
        }

        /// <inheritdoc />
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            Check.NotNull(node, nameof(node));

            if (node.Method.IsEFPropertyMethod())
            {
                Expression result = _queryModelVisitor.BindNavigationPathPropertyExpression(
                    node,
                    (ps, qs) =>
                    {
                        return qs != null
                            ? RewriteNavigationProperties(
                                ps,
                                qs,
                                node,
                                node.Arguments[0],
                                (string)((ConstantExpression)node.Arguments[1]).Value,
                                node.Type,
                                e => node.Arguments[0].Type != e.Type
                                    ? Expression.Call(node.Method, Expression.Convert(e, node.Arguments[0].Type), node.Arguments[1])
                                    : Expression.Call(node.Method, e, node.Arguments[1]),
                                e => node.Arguments[0].Type != e.Type
                                    ? new NullConditionalExpression(
                                        Expression.Convert(e, node.Arguments[0].Type),
                                        Expression.Call(node.Method, Expression.Convert(e, node.Arguments[0].Type), node.Arguments[1]))
                                    : new NullConditionalExpression(e, Expression.Call(node.Method, e, node.Arguments[1])))
                            : null;
                    });

                if (result != null)
                {
                    return result;
                }

                List<Expression> propertyArguments = VisitAndConvert(node.Arguments, nameof(VisitMethodCall)).ToList();

                MethodCallExpression newPropertyExpression = propertyArguments[0] != node.Arguments[0] || propertyArguments[1] != node.Arguments[1]
                    ? Expression.Call(node.Method, propertyArguments[0], node.Arguments[1])
                    : node;

                result = NeedsNullCompensation(propertyArguments[0])
                    ? (Expression)new NullConditionalExpression(propertyArguments[0], newPropertyExpression)
                    : newPropertyExpression;

                return result.Type == typeof(bool?) && node.Type == typeof(bool)
                    ? Expression.Equal(result, Expression.Constant(true, typeof(bool?)))
                    : result;
            }

            bool insideMaterializeCollectionNavigation = _insideMaterializeCollectionNavigation;
            if (node.Method.MethodIsClosedFormOf(CollectionNavigationSubqueryInjector.MaterializeCollectionNavigationMethodInfo))
            {
                _insideMaterializeCollectionNavigation = true;
            }

            Expression newObject = Visit(node.Object);
            List<Expression> newArguments = VisitAndConvert(node.Arguments, nameof(VisitMethodCall)).ToList();

            for (int i = 0; i < newArguments.Count; i++)
            {
                if (newArguments[i].Type != node.Arguments[i].Type)
                {
                    if (newArguments[i] is NullConditionalExpression nullConditionalArgument)
                    {
                        newArguments[i] = nullConditionalArgument.AccessOperation;
                    }

                    if (newArguments[i].Type != node.Arguments[i].Type)
                    {
                        newArguments[i] = Expression.Convert(newArguments[i], node.Arguments[i].Type);
                    }
                }
            }

            if (newObject != node.Object)
            {
                if (newObject is NullConditionalExpression nullConditionalExpression)
                {
                    MethodCallExpression newMethodCallExpression = node.Update(nullConditionalExpression.AccessOperation, newArguments);

                    return new NullConditionalExpression(newObject, newMethodCallExpression);
                }
            }

            MethodCallExpression newExpression = node.Update(newObject, newArguments);

            if (node.Method.MethodIsClosedFormOf(CollectionNavigationSubqueryInjector.MaterializeCollectionNavigationMethodInfo))
            {
                _insideMaterializeCollectionNavigation = insideMaterializeCollectionNavigation;
            }

            return newExpression;
        }

        private Expression RewriteNavigationProperties(
            IReadOnlyList<IPropertyBase> properties,
            IQuerySource querySource,
            Expression expression,
            Expression declaringExpression,
            string propertyName,
            Type propertyType,
            Func<Expression, Expression> propertyCreator,
            Func<Expression, Expression> conditionalAccessPropertyCreator)
        {
            List<INavigation> navigations = properties.OfType<INavigation>().ToList();

            if (navigations.Count > 0)
            {
                QuerySourceReferenceExpression outerQuerySourceReferenceExpression = new QuerySourceReferenceExpression(querySource);

                AdditionalFromClause additionalFromClauseBeingProcessed = _mongoDbNavigationRewritingQueryModelVisitor.AdditionalFromClauseBeingProcessed;
                if (additionalFromClauseBeingProcessed != null
                    && navigations.Last().IsCollection()
                    && !_insideMaterializeCollectionNavigation)
                {
                    if (additionalFromClauseBeingProcessed.FromExpression is SubQueryExpression fromSubqueryExpression)
                    {
                        if (fromSubqueryExpression.QueryModel.SelectClause.Selector is QuerySourceReferenceExpression)
                        {
                            return RewriteSelectManyInsideSubqueryIntoJoins(
                                fromSubqueryExpression,
                                outerQuerySourceReferenceExpression,
                                navigations,
                                additionalFromClauseBeingProcessed);
                        }
                    }
                    else
                    {
                        return RewriteSelectManyNavigationsIntoJoins(
                            outerQuerySourceReferenceExpression,
                            navigations,
                            additionalFromClauseBeingProcessed);
                    }
                }

                if (navigations.Count == 1
                    && navigations[0].IsDependentToPrincipal())
                {
                    Expression foreignKeyMemberAccess = TryCreateForeignKeyMemberAccess(propertyName, declaringExpression, navigations[0]);
                    if (foreignKeyMemberAccess != null)
                    {
                        return foreignKeyMemberAccess;
                    }
                }

                if (_insideInnerKeySelector && !_insideMaterializeCollectionNavigation)
                {
                    return CreateSubqueryForNavigations(
                        outerQuerySourceReferenceExpression,
                        properties,
                        propertyCreator);
                }

                Expression navigationResultExpression = RewriteNavigationsIntoJoins(
                    outerQuerySourceReferenceExpression,
                    navigations,
                    properties.Count == navigations.Count ? null : propertyType,
                    propertyCreator,
                    conditionalAccessPropertyCreator);

                if (navigationResultExpression is QuerySourceReferenceExpression resultQsre)
                {
                    foreach (IncludeResultOperator includeResultOperator in _queryModelVisitor.QueryCompilationContext.QueryAnnotations
                        .OfType<IncludeResultOperator>()
                        .Where(o => ExpressionEqualityComparer.Instance.Equals(o.PathFromQuerySource, expression)))
                    {
                        includeResultOperator.PathFromQuerySource = resultQsre;
                        includeResultOperator.QuerySource = resultQsre.ReferencedQuerySource;
                    }
                }

                return navigationResultExpression;
            }

            return default;
        }

        private class QsreWithNavigationFindingExpressionVisitor : ExpressionVisitorBase
        {
            private readonly QuerySourceReferenceExpression _searchedQsre;
            private readonly INavigation _navigation;
            private bool _navigationFound;

            public QsreWithNavigationFindingExpressionVisitor([NotNull] QuerySourceReferenceExpression searchedQsre, [NotNull] INavigation navigation)
            {
                _searchedQsre = searchedQsre;
                _navigation = navigation;
                _navigationFound = false;
                SearchedQsreFound = false;
            }

            public bool SearchedQsreFound { get; private set; }

            protected override Expression VisitMember(MemberExpression node)
            {
                if (!_navigationFound
                    && node.Member.Name == _navigation.Name)
                {
                    _navigationFound = true;

                    return base.VisitMember(node);
                }

                _navigationFound = false;

                return node;
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.IsEFPropertyMethod()
                    && !_navigationFound
                    && (string)((ConstantExpression)node.Arguments[1]).Value == _navigation.Name)
                {
                    _navigationFound = true;

                    return base.VisitMethodCall(node);
                }

                _navigationFound = false;

                return node;
            }

            protected override Expression VisitQuerySourceReference(QuerySourceReferenceExpression expression)
            {
                if (_navigationFound && expression.ReferencedQuerySource == _searchedQsre.ReferencedQuerySource)
                {
                    SearchedQsreFound = true;
                }
                else
                {
                    _navigationFound = false;
                }

                return expression;
            }
        }

        private Expression TryCreateForeignKeyMemberAccess(string propertyName, Expression declaringExpression, INavigation navigation)
        {
            bool canPerformOptimization = true;
            if (_insideOrderBy)
            {
                QuerySourceReferenceExpression qsre = (declaringExpression as MemberExpression)?.Expression as QuerySourceReferenceExpression;
                if (qsre == null)
                {
                    if (declaringExpression is MethodCallExpression methodCallExpression
                        && methodCallExpression.Method.IsEFPropertyMethod())
                    {
                        qsre = methodCallExpression.Arguments[0] as QuerySourceReferenceExpression;
                    }
                }

                if (qsre != null)
                {
                    QsreWithNavigationFindingExpressionVisitor qsreFindingVisitor = new QsreWithNavigationFindingExpressionVisitor(qsre, navigation);
                    qsreFindingVisitor.Visit(_queryModel.SelectClause.Selector);

                    if (qsreFindingVisitor.SearchedQsreFound)
                    {
                        canPerformOptimization = false;
                    }
                }
            }

            if (canPerformOptimization)
            {
                Expression foreignKeyMemberAccess = CreateForeignKeyMemberAccess(propertyName, declaringExpression, navigation);
                if (foreignKeyMemberAccess != null)
                {
                    return foreignKeyMemberAccess;
                }
            }

            return null;
        }

        private static Expression CreateForeignKeyMemberAccess(string propertyName, Expression declaringExpression, INavigation navigation)
        {
            IKey principalKey = navigation.ForeignKey.PrincipalKey;
            if (principalKey.Properties.Count == 1)
            {
                Debug.Assert(navigation.ForeignKey.Properties.Count == 1);

                IProperty principalKeyProperty = principalKey.Properties[0];
                if (principalKeyProperty.Name == propertyName
                    && principalKeyProperty.ClrType == navigation.ForeignKey.Properties[0].ClrType.UnwrapNullableType())
                {
                    Expression parentDeclaringExpression
                        = declaringExpression is MethodCallExpression declaringMethodCallExpression
                          && declaringMethodCallExpression.Method.IsEFPropertyMethod()
                            ? declaringMethodCallExpression.Arguments[0]
                            : (declaringExpression as MemberExpression)?.Expression;

                    if (parentDeclaringExpression != null)
                    {
                        Expression foreignKeyPropertyExpression = CreateKeyAccessExpression(parentDeclaringExpression, navigation.ForeignKey.Properties);

                        return foreignKeyPropertyExpression;
                    }
                }
            }

            return null;
        }

        private Expression CreateSubqueryForNavigations(
            Expression outerQuerySourceReferenceExpression,
            IReadOnlyList<IPropertyBase> properties,
            Func<Expression, Expression> propertyCreator)
        {
            List<INavigation> navigations = properties.OfType<INavigation>().ToList();
            INavigation firstNavigation = navigations.First();
            IEntityType targetEntityType = firstNavigation.GetTargetType();

            MainFromClause mainFromClause
                = new MainFromClause(
                    "subQuery",
                    targetEntityType.ClrType,
                    NullAsyncQueryProvider.Instance.CreateEntityQueryableExpression(targetEntityType.ClrType));

            _queryModelVisitor.QueryCompilationContext.AddOrUpdateMapping(mainFromClause, targetEntityType);
            QuerySourceReferenceExpression querySourceReference = new QuerySourceReferenceExpression(mainFromClause);
            QueryModel subQueryModel = new QueryModel(mainFromClause, new SelectClause(querySourceReference));

            Expression leftKeyAccess = CreateKeyAccessExpression(
                querySourceReference,
                firstNavigation.IsDependentToPrincipal()
                    ? firstNavigation.ForeignKey.PrincipalKey.Properties
                    : firstNavigation.ForeignKey.Properties);

            Expression rightKeyAccess = CreateKeyAccessExpression(
                outerQuerySourceReferenceExpression,
                firstNavigation.IsDependentToPrincipal()
                    ? firstNavigation.ForeignKey.Properties
                    : firstNavigation.ForeignKey.PrincipalKey.Properties);

            subQueryModel.BodyClauses.Add(
                new WhereClause(
                    CreateKeyComparisonExpressionForCollectionNavigationSubquery(
                        leftKeyAccess,
                        rightKeyAccess,
                        querySourceReference)));

            subQueryModel.ResultOperators.Add(new FirstResultOperator(returnDefaultWhenEmpty: true));

            Expression selectClauseExpression = (Expression)querySourceReference;

            selectClauseExpression
                = navigations
                    .Skip(1)
                    .Aggregate(
                        selectClauseExpression,
                        (current, navigation) => Expression.Property(current, navigation.Name));

            subQueryModel.SelectClause = new SelectClause(selectClauseExpression);

            if (properties.Count > navigations.Count)
            {
                subQueryModel.SelectClause = new SelectClause(propertyCreator(subQueryModel.SelectClause.Selector));
            }

            if (navigations.Count > 1)
            {
                NavigationRewritingExpressionVisitor subQueryVisitor = CreateVisitorForSubQuery(navigationExpansionSubquery: true);
                subQueryVisitor.Rewrite(subQueryModel, parentQueryModel: null);
            }

            return new SubQueryExpression(subQueryModel);
        }

        /// <inheritdoc />
        public override NavigationRewritingExpressionVisitor CreateVisitorForSubQuery(bool navigationExpansionSubquery)
            => new NavigationRewritingExpressionVisitor(
                _queryModelVisitor,
                navigationExpansionSubquery);

        private static Expression CreateKeyComparisonExpressionForCollectionNavigationSubquery(
            Expression leftExpression,
            Expression rightExpression,
            Expression leftQsre)
        {
            if (leftExpression.Type != rightExpression.Type)
            {
                if (leftExpression.Type.IsNullableType())
                {
                    Debug.Assert(leftExpression.Type.UnwrapNullableType() == rightExpression.Type);

                    rightExpression = Expression.Convert(rightExpression, leftExpression.Type);
                }
                else
                {
                    Debug.Assert(rightExpression.Type.IsNullableType());
                    Debug.Assert(rightExpression.Type.UnwrapNullableType() == leftExpression.Type);

                    leftExpression = Expression.Convert(leftExpression, rightExpression.Type);
                }
            }

            BinaryExpression outerNullProtection
                = Expression.NotEqual(
                    leftQsre,
                    Expression.Constant(null, leftQsre.Type));

            return new NullSafeEqualExpression(outerNullProtection, Expression.Equal(leftExpression, rightExpression));
        }

        private Expression RewriteNavigationsIntoJoins(
            QuerySourceReferenceExpression outerQuerySourceReferenceExpression,
            IEnumerable<INavigation> navigations,
            Type propertyType,
            Func<Expression, Expression> propertyCreator,
            Func<Expression, Expression> conditionalAccessPropertyCreator)
        {
            QuerySourceReferenceExpression sourceQsre = outerQuerySourceReferenceExpression;
            Expression sourceExpression = outerQuerySourceReferenceExpression;
            NavigationJoins navigationJoins = _navigationJoins;

            bool optionalNavigationInChain
                = NeedsNullCompensation(outerQuerySourceReferenceExpression);

            foreach (INavigation navigation in navigations)
            {
                bool addNullCheckToOuterKeySelector = optionalNavigationInChain;

                if (!navigation.ForeignKey.IsRequired
                    || !navigation.IsDependentToPrincipal()
                    || (navigation.DeclaringEntityType.ClrType != sourceExpression.Type
                        && navigation.DeclaringEntityType.GetAllBaseTypes().Any(t => t.ClrType == sourceExpression.Type)))
                {
                    optionalNavigationInChain = true;
                }

                if (!RewriteOwnedNavigations
                    && navigation.ForeignKey.IsOwnership)
                {
                    sourceExpression = Expression.Property(sourceExpression, navigation.PropertyInfo);

                    continue;
                }

                IEntityType targetEntityType = navigation.GetTargetType();

                if (navigation.IsCollection())
                {
                    _queryModel.MainFromClause.FromExpression
                        = NullAsyncQueryProvider.Instance.CreateEntityQueryableExpression(targetEntityType.ClrType);

                    QuerySourceReferenceExpression innerQuerySourceReferenceExpression
                        = new QuerySourceReferenceExpression(_queryModel.MainFromClause);
                    _queryModelVisitor.QueryCompilationContext.AddOrUpdateMapping(_queryModel.MainFromClause, targetEntityType);

                    Expression leftKeyAccess = CreateKeyAccessExpression(
                        sourceExpression,
                        navigation.IsDependentToPrincipal()
                            ? navigation.ForeignKey.Properties
                            : navigation.ForeignKey.PrincipalKey.Properties);

                    Expression rightKeyAccess = CreateKeyAccessExpression(
                        innerQuerySourceReferenceExpression,
                        navigation.IsDependentToPrincipal()
                            ? navigation.ForeignKey.PrincipalKey.Properties
                            : navigation.ForeignKey.Properties);

                    _queryModel.BodyClauses.Add(
                        new WhereClause(
                            CreateKeyComparisonExpressionForCollectionNavigationSubquery(
                                leftKeyAccess,
                                rightKeyAccess,
                                sourceExpression)));

                    return _queryModel.MainFromClause.FromExpression;
                }

                NavigationJoin navigationJoin
                    = navigationJoins
                        .FirstOrDefault(nj =>
                            nj.QuerySource == (sourceExpression as QuerySourceReferenceExpression ?? sourceQsre).ReferencedQuerySource
                            && nj.Navigation == navigation);

                if (navigationJoin == null)
                {
                    JoinClause joinClause = BuildJoinFromNavigation(
                        sourceExpression,
                        navigation,
                        targetEntityType,
                        addNullCheckToOuterKeySelector,
                        out QuerySourceReferenceExpression innerQuerySourceReferenceExpression);

                    if (optionalNavigationInChain)
                    {
                        RewriteNavigationIntoGroupJoin(
                            joinClause,
                            navigation,
                            targetEntityType,
                            sourceExpression as QuerySourceReferenceExpression ?? sourceQsre,
                            null,
                            new List<IBodyClause>(),
                            new List<ResultOperatorBase>
                            {
                                new DefaultIfEmptyResultOperator(null)
                            },
                            out navigationJoin);
                    }
                    else
                    {
                        navigationJoin
                            = new NavigationJoin(
                                (sourceExpression as QuerySourceReferenceExpression ?? sourceQsre).ReferencedQuerySource,
                                navigation,
                                joinClause,
                                new List<IBodyClause>(),
                                navigation.IsDependentToPrincipal(),
                                innerQuerySourceReferenceExpression);
                    }
                }

                navigationJoins.Add(navigationJoin);

                sourceExpression = navigationJoin.QuerySourceReferenceExpression;
                sourceQsre = navigationJoin.QuerySourceReferenceExpression;
                navigationJoins = navigationJoin.Children;
            }

            return propertyType == null
                ? sourceExpression
                : optionalNavigationInChain
                ? conditionalAccessPropertyCreator(sourceExpression)
                : propertyCreator(sourceExpression);
        }

        private void RewriteNavigationIntoGroupJoin(
            JoinClause joinClause,
            INavigation navigation,
            IEntityType targetEntityType,
            QuerySourceReferenceExpression querySourceReferenceExpression,
            MainFromClause groupJoinSubqueryMainFromClause,
            ICollection<IBodyClause> groupJoinSubqueryBodyClauses,
            ICollection<ResultOperatorBase> groupJoinSubqueryResultOperators,
            out NavigationJoin navigationJoin)
        {
            GroupJoinClause groupJoinClause
                = new GroupJoinClause(
                    joinClause.ItemName + "_group",
                    typeof(IEnumerable<>).MakeGenericType(targetEntityType.ClrType),
                    joinClause);

            QuerySourceReferenceExpression groupReferenceExpression = new QuerySourceReferenceExpression(groupJoinClause);

            MainFromClause groupJoinSubqueryModelMainFromClause = new MainFromClause(joinClause.ItemName + "_groupItem", joinClause.ItemType, groupReferenceExpression);
            QuerySourceReferenceExpression newQuerySourceReferenceExpression = new QuerySourceReferenceExpression(groupJoinSubqueryModelMainFromClause);
            _queryModelVisitor.QueryCompilationContext.AddOrUpdateMapping(groupJoinSubqueryModelMainFromClause, targetEntityType);

            QueryModel groupJoinSubqueryModel = new QueryModel(
                groupJoinSubqueryModelMainFromClause,
                new SelectClause(newQuerySourceReferenceExpression));

            foreach (IBodyClause groupJoinSubqueryBodyClause in groupJoinSubqueryBodyClauses)
            {
                groupJoinSubqueryModel.BodyClauses.Add(groupJoinSubqueryBodyClause);
            }

            foreach (ResultOperatorBase groupJoinSubqueryResultOperator in groupJoinSubqueryResultOperators)
            {
                groupJoinSubqueryModel.ResultOperators.Add(groupJoinSubqueryResultOperator);
            }

            if (groupJoinSubqueryMainFromClause != null
                && (groupJoinSubqueryBodyClauses.Any() || groupJoinSubqueryResultOperators.Any()))
            {
                QuerySourceMapping querySourceMapping = new QuerySourceMapping();
                querySourceMapping.AddMapping(groupJoinSubqueryMainFromClause, newQuerySourceReferenceExpression);

                groupJoinSubqueryModel.TransformExpressions(
                    e => ReferenceReplacingExpressionVisitor
                        .ReplaceClauseReferences(e, querySourceMapping, throwOnUnmappedReferences: false));
            }

            SubQueryExpression defaultIfEmptySubquery = new SubQueryExpression(groupJoinSubqueryModel);
            AdditionalFromClause defaultIfEmptyAdditionalFromClause = new AdditionalFromClause(joinClause.ItemName, joinClause.ItemType, defaultIfEmptySubquery);

            navigationJoin = new NavigationJoin(
                querySourceReferenceExpression.ReferencedQuerySource,
                navigation,
                groupJoinClause,
                new[] { defaultIfEmptyAdditionalFromClause },
                navigation.IsDependentToPrincipal(),
                new QuerySourceReferenceExpression(defaultIfEmptyAdditionalFromClause));

            _queryModelVisitor.QueryCompilationContext.AddOrUpdateMapping(defaultIfEmptyAdditionalFromClause, targetEntityType);
        }

        private Expression RewriteSelectManyNavigationsIntoJoins(
            QuerySourceReferenceExpression outerQuerySourceReferenceExpression,
            IEnumerable<INavigation> navigations,
            AdditionalFromClause additionalFromClauseBeingProcessed)
        {
            Expression sourceExpression = outerQuerySourceReferenceExpression;
            QuerySourceReferenceExpression querySourceReferenceExpression = outerQuerySourceReferenceExpression;
            int additionalJoinIndex = _queryModel.BodyClauses.IndexOf(additionalFromClauseBeingProcessed);
            List<JoinClause> joinClauses = new List<JoinClause>();

            foreach (INavigation navigation in navigations)
            {
                IEntityType targetEntityType = navigation.GetTargetType();

                if (!RewriteOwnedNavigations && navigation.ForeignKey.IsOwnership)
                {
                    sourceExpression = Expression.Property(sourceExpression, navigation.PropertyInfo);

                    continue;
                }

                JoinClause joinClause = BuildJoinFromNavigation(
                    sourceExpression,
                    navigation,
                    targetEntityType,
                    false,
                    out QuerySourceReferenceExpression innerQuerySourceReferenceExpression);

                joinClauses.Add(joinClause);

                querySourceReferenceExpression = innerQuerySourceReferenceExpression;
                sourceExpression = innerQuerySourceReferenceExpression;
            }

            if (RewriteOwnedNavigations || !navigations.Last().ForeignKey.IsOwnership)
            {
                _queryModel.BodyClauses.RemoveAt(additionalJoinIndex);
            }
            else
            {
                ((AdditionalFromClause)_queryModel.BodyClauses[additionalJoinIndex]).FromExpression = sourceExpression;
            }

            for (int i = 0; i < joinClauses.Count; i++)
            {
                _queryModel.BodyClauses.Insert(additionalJoinIndex + i, joinClauses[i]);
            }

            if (RewriteOwnedNavigations || !navigations.Last().ForeignKey.IsOwnership)
            {
                QuerySourceMapping querySourceMapping = new QuerySourceMapping();
                querySourceMapping.AddMapping(additionalFromClauseBeingProcessed, querySourceReferenceExpression);

                _queryModel.TransformExpressions(
                    e => ReferenceReplacingExpressionVisitor
                        .ReplaceClauseReferences(e, querySourceMapping, throwOnUnmappedReferences: false));

                AdjustQueryCompilationContextStateAfterSelectMany(
                    querySourceMapping,
                    additionalFromClauseBeingProcessed,
                    querySourceReferenceExpression.ReferencedQuerySource);
            }

            return RewriteOwnedNavigations || !navigations.Last().ForeignKey.IsOwnership
                ? querySourceReferenceExpression
                : sourceExpression;
        }

        private Expression RewriteSelectManyInsideSubqueryIntoJoins(
            SubQueryExpression fromSubqueryExpression,
            QuerySourceReferenceExpression outerQuerySourceReferenceExpression,
            ICollection<INavigation> navigations,
            AdditionalFromClause additionalFromClauseBeingProcessed)
        {
            INavigation collectionNavigation = navigations.Last();
            List<IBodyClause> adddedJoinClauses = new List<IBodyClause>();

            foreach (INavigation navigation in navigations)
            {
                IEntityType targetEntityType = navigation.GetTargetType();

                JoinClause joinClause = BuildJoinFromNavigation(
                    outerQuerySourceReferenceExpression,
                    navigation,
                    targetEntityType,
                    false,
                    out QuerySourceReferenceExpression innerQuerySourceReferenceExpression);

                if (navigation == collectionNavigation)
                {
                    RewriteNavigationIntoGroupJoin(
                        joinClause,
                        navigations.Last(),
                        targetEntityType,
                        outerQuerySourceReferenceExpression,
                        fromSubqueryExpression.QueryModel.MainFromClause,
                        fromSubqueryExpression.QueryModel.BodyClauses,
                        fromSubqueryExpression.QueryModel.ResultOperators,
                        out NavigationJoin navigationJoin);

                    _navigationJoins.Add(navigationJoin);

                    int additionalFromClauseIndex = _parentQueryModel.BodyClauses.IndexOf(additionalFromClauseBeingProcessed);
                    _parentQueryModel.BodyClauses.Remove(additionalFromClauseBeingProcessed);

                    int i = additionalFromClauseIndex;
                    foreach (IBodyClause addedJoinClause in adddedJoinClauses)
                    {
                        _parentQueryModel.BodyClauses.Insert(i++, addedJoinClause);
                    }

                    QuerySourceMapping querySourceMapping = new QuerySourceMapping();
                    querySourceMapping.AddMapping(additionalFromClauseBeingProcessed, navigationJoin.QuerySourceReferenceExpression);

                    _parentQueryModel.TransformExpressions(
                        e => ReferenceReplacingExpressionVisitor
                            .ReplaceClauseReferences(e, querySourceMapping, throwOnUnmappedReferences: false));

                    AdjustQueryCompilationContextStateAfterSelectMany(
                        querySourceMapping,
                        additionalFromClauseBeingProcessed,
                        navigationJoin.QuerySourceReferenceExpression.ReferencedQuerySource);

                    return navigationJoin.QuerySourceReferenceExpression;
                }

                adddedJoinClauses.Add(joinClause);

                outerQuerySourceReferenceExpression = innerQuerySourceReferenceExpression;
            }

            return outerQuerySourceReferenceExpression;
        }

        private void AdjustQueryCompilationContextStateAfterSelectMany(QuerySourceMapping querySourceMapping, IQuerySource querySourceBeingProcessed, IQuerySource resultQuerySource)
        {
            foreach (IncludeResultOperator includeResultOperator in _queryModelVisitor.QueryCompilationContext.QueryAnnotations.OfType<IncludeResultOperator>())
            {
                includeResultOperator.PathFromQuerySource
                    = ReferenceReplacingExpressionVisitor.ReplaceClauseReferences(
                        includeResultOperator.PathFromQuerySource,
                        querySourceMapping,
                        throwOnUnmappedReferences: false);

                if (includeResultOperator.QuerySource == querySourceBeingProcessed)
                {
                    includeResultOperator.QuerySource = resultQuerySource;
                }
            }
        }

        private JoinClause BuildJoinFromNavigation(
            Expression sourceExpression,
            INavigation navigation,
            IEntityType targetEntityType,
            bool addNullCheckToOuterKeySelector,
            out QuerySourceReferenceExpression innerQuerySourceReferenceExpression)
        {
            Expression outerKeySelector =
                CreateKeyAccessExpression(
                    sourceExpression,
                    navigation.IsDependentToPrincipal()
                        ? navigation.ForeignKey.Properties
                        : navigation.ForeignKey.PrincipalKey.Properties,
                    addNullCheckToOuterKeySelector);

            string itemName = sourceExpression is QuerySourceReferenceExpression qsre && !qsre.ReferencedQuerySource.HasGeneratedItemName()
                ? qsre.ReferencedQuerySource.ItemName
                : navigation.DeclaringEntityType.ShortName()[0].ToString().ToLowerInvariant();

            JoinClause joinClause
                = new JoinClause(
                    $"{itemName}.{navigation.Name}",
                    targetEntityType.ClrType,
                    NullAsyncQueryProvider.Instance.CreateEntityQueryableExpression(targetEntityType.ClrType),
                    outerKeySelector,
                    Expression.Constant(null));

            innerQuerySourceReferenceExpression = new QuerySourceReferenceExpression(joinClause);
            _queryModelVisitor.QueryCompilationContext.AddOrUpdateMapping(joinClause, targetEntityType);

            Expression innerKeySelector
                = CreateKeyAccessExpression(
                    innerQuerySourceReferenceExpression,
                    navigation.IsDependentToPrincipal()
                        ? navigation.ForeignKey.PrincipalKey.Properties
                        : navigation.ForeignKey.Properties);

            if (innerKeySelector.Type != joinClause.OuterKeySelector.Type)
            {
                if (innerKeySelector.Type.IsNullableType())
                {
                    joinClause.OuterKeySelector
                        = Expression.Convert(
                            joinClause.OuterKeySelector,
                            innerKeySelector.Type);
                }
                else
                {
                    innerKeySelector
                        = Expression.Convert(
                            innerKeySelector,
                            joinClause.OuterKeySelector.Type);
                }
            }

            joinClause.InnerKeySelector = innerKeySelector;

            return joinClause;
        }

        private static Expression CreateKeyAccessExpression(
            Expression target, IReadOnlyList<IProperty> properties, bool addNullCheck = false)
            => properties.Count == 1
                ? CreatePropertyExpression(target, properties[0], addNullCheck)
                : Expression.New(
                    AnonymousObject.AnonymousObjectCtor,
                    Expression.NewArrayInit(
                        typeof(object),
                        properties
                            .Select(p => Expression.Convert(CreatePropertyExpression(target, p, addNullCheck), typeof(object)))
                            .Cast<Expression>()
                            .ToArray()));

        private static Expression CreatePropertyExpression(Expression target, IProperty property, bool addNullCheck)
        {
            Expression propertyExpression = target.CreateEFPropertyExpression(property, makeNullable: false);

            Type propertyDeclaringType = property.DeclaringType.ClrType;
            if (propertyDeclaringType != target.Type
                && target.Type.GetTypeInfo().IsAssignableFrom(propertyDeclaringType.GetTypeInfo()))
            {
                if (!propertyExpression.Type.IsNullableType())
                {
                    propertyExpression = Expression.Convert(propertyExpression, propertyExpression.Type.MakeNullable());
                }

                return Expression.Condition(
                    Expression.TypeIs(target, propertyDeclaringType),
                    propertyExpression,
                    Expression.Constant(null, propertyExpression.Type));
            }

            return addNullCheck
                ? new NullConditionalExpression(target, propertyExpression)
                : propertyExpression;
        }

        private static bool IsCompositeKey([NotNull] Type type)
        {
            Check.NotNull(type, nameof(type));

            return type == typeof(AnonymousObject);
        }

        private static Expression CompensateForNullabilityDifference(Expression expression, Type originalType)
        {
            Type newType = expression.Type;

            bool needsTypeCompensation
                = originalType != newType
                  && !originalType.IsNullableType()
                  && newType.IsNullableType()
                  && originalType == newType.UnwrapNullableType();

            return needsTypeCompensation
                ? Expression.Convert(expression, originalType)
                : expression;
        }

        private class MongoDbNavigationRewritingQueryModelVisitor : ExpressionTransformingQueryModelVisitor<MongoDbNavigationRewritingExpressionVisitor>
        {
            private readonly CollectionNavigationSubqueryInjector _subqueryInjector;
            private readonly bool _navigationExpansionSubquery;
            private readonly QueryCompilationContext _queryCompilationContext;

            public AdditionalFromClause AdditionalFromClauseBeingProcessed { get; private set; }

            public MongoDbNavigationRewritingQueryModelVisitor(
                MongoDbNavigationRewritingExpressionVisitor transformingVisitor,
                EntityQueryModelVisitor queryModelVisitor,
                bool navigationExpansionSubquery)
                : base(transformingVisitor)
            {
                _subqueryInjector = new CollectionNavigationSubqueryInjector(queryModelVisitor, shouldInject: true);
                _navigationExpansionSubquery = navigationExpansionSubquery;
                _queryCompilationContext = queryModelVisitor.QueryCompilationContext;
            }

            public override void VisitMainFromClause(MainFromClause fromClause, QueryModel queryModel)
            {
                base.VisitMainFromClause(fromClause, queryModel);

                QueryCompilationContext queryCompilationContext = TransformingVisitor._queryModelVisitor.QueryCompilationContext;
                if (queryCompilationContext.FindEntityType(fromClause) == null
                    && fromClause.FromExpression is SubQueryExpression subQuery)
                {
                    IEntityType entityType = MemberAccessBindingExpressionVisitor.GetEntityType(
                        subQuery.QueryModel.SelectClause.Selector, queryCompilationContext);

                    if (entityType != null)
                    {
                        queryCompilationContext.AddOrUpdateMapping(fromClause, entityType);
                    }
                }
            }

            public override void VisitAdditionalFromClause(AdditionalFromClause fromClause, QueryModel queryModel, int index)
            {
                // ReSharper disable once PatternAlwaysOfType
                if (fromClause.TryGetFlattenedGroupJoinClause()?.JoinClause is JoinClause joinClause
                    // ReSharper disable once PatternAlwaysOfType
                    && _queryCompilationContext.FindEntityType(joinClause) is IEntityType entityType)
                {
                    _queryCompilationContext.AddOrUpdateMapping(fromClause, entityType);
                }

                AdditionalFromClause oldAdditionalFromClause = AdditionalFromClauseBeingProcessed;
                AdditionalFromClauseBeingProcessed = fromClause;
                fromClause.TransformExpressions(TransformingVisitor.Visit);
                AdditionalFromClauseBeingProcessed = oldAdditionalFromClause;
            }

            public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index)
            {
                base.VisitWhereClause(whereClause, queryModel, index);

                if (whereClause.Predicate.Type == typeof(bool?))
                {
                    whereClause.Predicate = Expression.Equal(whereClause.Predicate, Expression.Constant(true, typeof(bool?)));
                }
            }

            public override void VisitOrderByClause(OrderByClause orderByClause, QueryModel queryModel, int index)
            {
                List<Type> originalTypes = orderByClause.Orderings.Select(o => o.Expression.Type).ToList();

                bool oldInsideOrderBy = TransformingVisitor._insideOrderBy;
                TransformingVisitor._insideOrderBy = true;

                base.VisitOrderByClause(orderByClause, queryModel, index);

                TransformingVisitor._insideOrderBy = oldInsideOrderBy;

                for (int i = 0; i < orderByClause.Orderings.Count; i++)
                {
                    orderByClause.Orderings[i].Expression = CompensateForNullabilityDifference(
                        orderByClause.Orderings[i].Expression,
                        originalTypes[i]);
                }
            }

            public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, int index)
                => VisitJoinClauseInternal(joinClause);

            public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, GroupJoinClause groupJoinClause)
                => VisitJoinClauseInternal(joinClause);

            private void VisitJoinClauseInternal(JoinClause joinClause)
            {
                joinClause.InnerSequence = TransformingVisitor.Visit(joinClause.InnerSequence);

                QueryCompilationContext queryCompilationContext = TransformingVisitor._queryModelVisitor.QueryCompilationContext;
                if (queryCompilationContext.FindEntityType(joinClause) == null
                    && joinClause.InnerSequence is SubQueryExpression subQuery)
                {
                    IEntityType entityType = MemberAccessBindingExpressionVisitor.GetEntityType(
                        subQuery.QueryModel.SelectClause.Selector, queryCompilationContext);
                    if (entityType != null)
                    {
                        queryCompilationContext.AddOrUpdateMapping(joinClause, entityType);
                    }
                }

                joinClause.OuterKeySelector = TransformingVisitor.Visit(joinClause.OuterKeySelector);

                bool oldInsideInnerKeySelector = TransformingVisitor._insideInnerKeySelector;
                TransformingVisitor._insideInnerKeySelector = true;
                joinClause.InnerKeySelector = TransformingVisitor.Visit(joinClause.InnerKeySelector);

                if (joinClause.OuterKeySelector.Type.IsNullableType()
                    && !joinClause.InnerKeySelector.Type.IsNullableType())
                {
                    joinClause.InnerKeySelector = Expression.Convert(joinClause.InnerKeySelector, joinClause.InnerKeySelector.Type.MakeNullable());
                }

                if (joinClause.InnerKeySelector.Type.IsNullableType()
                    && !joinClause.OuterKeySelector.Type.IsNullableType())
                {
                    joinClause.OuterKeySelector = Expression.Convert(joinClause.OuterKeySelector, joinClause.OuterKeySelector.Type.MakeNullable());
                }

                TransformingVisitor._insideInnerKeySelector = oldInsideInnerKeySelector;
            }

            public override void VisitSelectClause(SelectClause selectClause, QueryModel queryModel)
            {
                selectClause.Selector = _subqueryInjector.Visit(selectClause.Selector);

                if (_navigationExpansionSubquery)
                {
                    base.VisitSelectClause(selectClause, queryModel);
                    return;
                }

                Type originalType = selectClause.Selector.Type;

                base.VisitSelectClause(selectClause, queryModel);

                selectClause.Selector = CompensateForNullabilityDifference(selectClause.Selector, originalType);
            }

            public override void VisitResultOperator(ResultOperatorBase resultOperator, QueryModel queryModel, int index)
            {
                if (resultOperator is AllResultOperator allResultOperator)
                {
                    Expression expressionExtractor(AllResultOperator o) => o.Predicate;
                    void adjuster(AllResultOperator o, Expression e) => o.Predicate = e;
                    VisitAndAdjustResultOperatorType(allResultOperator, expressionExtractor, adjuster);

                    return;
                }

                if (resultOperator is ContainsResultOperator containsResultOperator)
                {
                    Expression expressionExtractor(ContainsResultOperator o) => o.Item;
                    void adjuster(ContainsResultOperator o, Expression e) => o.Item = e;
                    VisitAndAdjustResultOperatorType(containsResultOperator, expressionExtractor, adjuster);

                    return;
                }

                if (resultOperator is SkipResultOperator skipResultOperator)
                {
                    Expression expressionExtractor(SkipResultOperator o) => o.Count;
                    void adjuster(SkipResultOperator o, Expression e) => o.Count = e;
                    VisitAndAdjustResultOperatorType(skipResultOperator, expressionExtractor, adjuster);

                    return;
                }

                if (resultOperator is TakeResultOperator takeResultOperator)
                {
                    Expression expressionExtractor(TakeResultOperator o) => o.Count;
                    void adjuster(TakeResultOperator o, Expression e) => o.Count = e;
                    VisitAndAdjustResultOperatorType(takeResultOperator, expressionExtractor, adjuster);

                    return;
                }

                if (resultOperator is GroupResultOperator groupResultOperator)
                {
                    groupResultOperator.ElementSelector
                        = _subqueryInjector.Visit(groupResultOperator.ElementSelector);

                    Type originalKeySelectorType = groupResultOperator.KeySelector.Type;
                    Type originalElementSelectorType = groupResultOperator.ElementSelector.Type;

                    base.VisitResultOperator(resultOperator, queryModel, index);

                    groupResultOperator.KeySelector = CompensateForNullabilityDifference(
                        groupResultOperator.KeySelector,
                        originalKeySelectorType);

                    groupResultOperator.ElementSelector = CompensateForNullabilityDifference(
                        groupResultOperator.ElementSelector,
                        originalElementSelectorType);

                    return;
                }

                base.VisitResultOperator(resultOperator, queryModel, index);
            }

            private void VisitAndAdjustResultOperatorType<TResultOperator>(
                TResultOperator resultOperator,
                Func<TResultOperator, Expression> expressionExtractor,
                Action<TResultOperator, Expression> adjuster)
                where TResultOperator : ResultOperatorBase
            {
                Expression originalExpression = expressionExtractor(resultOperator);
                Type originalType = originalExpression.Type;

                Expression translatedExpression = CompensateForNullabilityDifference(
                    TransformingVisitor.Visit(originalExpression),
                    originalType);

                adjuster(resultOperator, translatedExpression);
            }
        }
    }
}