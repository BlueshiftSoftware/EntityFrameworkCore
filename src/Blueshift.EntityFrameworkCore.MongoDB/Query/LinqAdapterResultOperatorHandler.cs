using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query
{
    using ResultHandler = Func<MongoDbEntityQueryModelVisitor, ResultOperatorBase, QueryModel, Expression>;

    /// <summary>
    ///     The default client-eval result operator handler.
    /// </summary>
    public class LinqAdapterResultOperatorHandler : ResultOperatorHandler
    {
        [NotNull] private readonly IQueryableMethodProvider _queryableMethodProvider;

        private static readonly Dictionary<Type, ResultHandler> _handlers
            = new Dictionary<Type, ResultHandler>
            {
                { typeof(AllResultOperator), (v, r, q) => HandleAll(v, (AllResultOperator)r, q) },
                { typeof(AnyResultOperator), (v, _, __) => HandleAny(v) },
                { typeof(AverageResultOperator), (v, _, __) => HandleAverage(v) },
                { typeof(CastResultOperator), (v, r, __) => HandleCast(v, (CastResultOperator)r) },
                { typeof(ConcatResultOperator), (v, r, __) => HandleConcat(v, (ConcatResultOperator)r) },
                { typeof(CountResultOperator), (v, _, __) => HandleCount(v) },
                { typeof(ContainsResultOperator), (v, r, q) => HandleContains(v, (ContainsResultOperator)r, q) },
                { typeof(DefaultIfEmptyResultOperator), (v, r, q) => HandleDefaultIfEmpty(v, (DefaultIfEmptyResultOperator)r, q) },
                { typeof(DistinctResultOperator), (v, _, __) => HandleDistinct(v) },
                { typeof(ExceptResultOperator), (v, r, __) => HandleExcept(v, (ExceptResultOperator)r) },
                { typeof(FirstResultOperator), (v, r, __) => HandleFirst(v, (ChoiceResultOperatorBase)r) },
                { typeof(IntersectResultOperator), (v, r, __) => HandleIntersect(v, (IntersectResultOperator)r) },
                { typeof(GroupResultOperator), (v, r, q) => HandleGroup(v, (GroupResultOperator)r, q) },
                { typeof(LastResultOperator), (v, r, __) => HandleLast(v, (ChoiceResultOperatorBase)r) },
                { typeof(LongCountResultOperator), (v, _, __) => HandleLongCount(v) },
                { typeof(MinResultOperator), (v, _, __) => HandleMin(v) },
                { typeof(MaxResultOperator), (v, _, __) => HandleMax(v) },
                { typeof(OfTypeResultOperator), (v, r, q) => HandleOfType(v, (OfTypeResultOperator)r) },
                { typeof(SingleResultOperator), (v, r, __) => HandleSingle(v, (ChoiceResultOperatorBase)r) },
                { typeof(SkipResultOperator), (v, r, __) => HandleSkip(v, (SkipResultOperator)r) },
                { typeof(SumResultOperator), (v, _, __) => HandleSum(v) },
                { typeof(TakeResultOperator), (v, r, __) => HandleTake(v, (TakeResultOperator)r) },
                { typeof(UnionResultOperator), (v, r, __) => HandleUnion(v, (UnionResultOperator)r) }
            };

        /// <summary>
        ///     Initializes a new instance of the <see cref="ResultOperatorHandler" /> class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        /// <param name="queryableMethodProvider">The <see cref="IQueryableMethodProvider"/> to use for referencing <see cref="IQueryable"/> methods.</param>
        public LinqAdapterResultOperatorHandler(
            [NotNull] ResultOperatorHandlerDependencies dependencies,
            [NotNull] IQueryableMethodProvider queryableMethodProvider)
            : base(Check.NotNull(dependencies, nameof(dependencies)))
        {
            _queryableMethodProvider = Check.NotNull(queryableMethodProvider, nameof(queryableMethodProvider));
        }

        /// <summary>
        ///     Handles the result operator.
        /// </summary>
        /// <param name="entityQueryModelVisitor"> The entity query model visitor. </param>
        /// <param name="resultOperator"> The result operator. </param>
        /// <param name="queryModel"> The query model. </param>
        /// <returns>
        ///     An compiled query expression fragment representing the result operator.
        /// </returns>
        public override Expression HandleResultOperator(
            EntityQueryModelVisitor entityQueryModelVisitor,
            ResultOperatorBase resultOperator,
            QueryModel queryModel)
        {
            Check.NotNull(entityQueryModelVisitor, nameof(entityQueryModelVisitor));
            Check.NotNull(resultOperator, nameof(resultOperator));
            Check.NotNull(queryModel, nameof(queryModel));

            Expression expression;

            if (_queryableMethodProvider.IsQueryableExpression(entityQueryModelVisitor.Expression)
                && _handlers.TryGetValue(resultOperator.GetType(), out var handler))
            {
                expression = handler(
                    entityQueryModelVisitor as MongoDbEntityQueryModelVisitor,
                    resultOperator,
                    queryModel);
            }
            else
            {
                expression = base.HandleResultOperator(entityQueryModelVisitor, resultOperator, queryModel);
            }

            return expression;
        }

        private static Expression HandleAll(
            MongoDbEntityQueryModelVisitor mongoDbEntityQueryModelVisitor,
            AllResultOperator allResultOperator,
            QueryModel queryModel)
        {
            var sequenceType
                = mongoDbEntityQueryModelVisitor.Expression.Type.GetSequenceType();

            var predicate
                = mongoDbEntityQueryModelVisitor
                    .ReplaceClauseReferences(
                        allResultOperator.Predicate,
                        queryModel.MainFromClause);

            return CallWithPossibleCancellationToken(
                mongoDbEntityQueryModelVisitor.QueryableMethodProvider.All
                    .MakeGenericMethod(sequenceType),
                mongoDbEntityQueryModelVisitor.Expression,
                Expression.Lambda(predicate, mongoDbEntityQueryModelVisitor.CurrentParameter));
        }

        private static Expression HandleAny(MongoDbEntityQueryModelVisitor mongoDbEntityQueryModelVisitor)
            => CallWithPossibleCancellationToken(
                mongoDbEntityQueryModelVisitor.QueryableMethodProvider.Any
                    .MakeGenericMethod(mongoDbEntityQueryModelVisitor.Expression.Type.GetSequenceType()),
                mongoDbEntityQueryModelVisitor.Expression);

        private static Expression HandleAverage(MongoDbEntityQueryModelVisitor mongoDbEntityQueryModelVisitor)
            => HandleAggregate(mongoDbEntityQueryModelVisitor, "Average");

        private static Expression HandleCast(
            MongoDbEntityQueryModelVisitor mongoDbEntityQueryModelVisitor,
            CastResultOperator castResultOperator)
        {
            var resultItemTypeInfo
                = mongoDbEntityQueryModelVisitor.Expression.Type
                    .GetSequenceType().GetTypeInfo();

            if (castResultOperator.CastItemType.GetTypeInfo()
                .IsAssignableFrom(resultItemTypeInfo))
            {
                return mongoDbEntityQueryModelVisitor.Expression;
            }

            return Expression.Call(
                mongoDbEntityQueryModelVisitor.QueryableMethodProvider
                    .Cast.MakeGenericMethod(castResultOperator.CastItemType),
                mongoDbEntityQueryModelVisitor.Expression);
        }

        private static Expression HandleConcat(
            MongoDbEntityQueryModelVisitor mongoDbEntityQueryModelVisitor,
            ConcatResultOperator concatResultOperator)
            => HandleSetOperation(
                mongoDbEntityQueryModelVisitor,
                concatResultOperator.Source2,
                mongoDbEntityQueryModelVisitor.QueryableMethodProvider.Concat);

        private static Expression HandleCount(MongoDbEntityQueryModelVisitor mongoDbEntityQueryModelVisitor)
            => CallWithPossibleCancellationToken(
                mongoDbEntityQueryModelVisitor.QueryableMethodProvider
                    .Count.MakeGenericMethod(mongoDbEntityQueryModelVisitor.Expression.Type.GetSequenceType()),
                mongoDbEntityQueryModelVisitor.Expression);

        private static Expression HandleContains(
            MongoDbEntityQueryModelVisitor mongoDbEntityQueryModelVisitor,
            ContainsResultOperator containsResultOperator,
            QueryModel queryModel)
        {
            var item
                = mongoDbEntityQueryModelVisitor
                    .ReplaceClauseReferences(
                        containsResultOperator.Item,
                        queryModel.MainFromClause);

            return CallWithPossibleCancellationToken(
                mongoDbEntityQueryModelVisitor.QueryableMethodProvider.Contains
                    .MakeGenericMethod(mongoDbEntityQueryModelVisitor.Expression.Type.GetSequenceType()),
                mongoDbEntityQueryModelVisitor.Expression,
                item);
        }

        private static Expression HandleDefaultIfEmpty(
            MongoDbEntityQueryModelVisitor mongoDbEntityQueryModelVisitor,
            DefaultIfEmptyResultOperator defaultIfEmptyResultOperator,
            QueryModel queryModel)
        {
            if (defaultIfEmptyResultOperator.OptionalDefaultValue == null)
            {
                return Expression.Call(
                    mongoDbEntityQueryModelVisitor.QueryableMethodProvider.DefaultIfEmpty
                        .MakeGenericMethod(mongoDbEntityQueryModelVisitor.Expression.Type.GetSequenceType()),
                    mongoDbEntityQueryModelVisitor.Expression);
            }

            var optionalDefaultValue
                = mongoDbEntityQueryModelVisitor
                    .ReplaceClauseReferences(
                        defaultIfEmptyResultOperator.OptionalDefaultValue,
                        queryModel.MainFromClause);

            return Expression.Call(
                mongoDbEntityQueryModelVisitor.QueryableMethodProvider.DefaultIfEmptyParameter
                    .MakeGenericMethod(mongoDbEntityQueryModelVisitor.Expression.Type.GetSequenceType()),
                mongoDbEntityQueryModelVisitor.Expression,
                optionalDefaultValue);
        }

        private static Expression HandleDistinct(MongoDbEntityQueryModelVisitor mongoDbEntityQueryModelVisitor)
            => Expression.Call(
                mongoDbEntityQueryModelVisitor.QueryableMethodProvider.Distinct
                    .MakeGenericMethod(mongoDbEntityQueryModelVisitor.Expression.Type.GetSequenceType()),
                mongoDbEntityQueryModelVisitor.Expression);

        private static Expression HandleExcept(
            MongoDbEntityQueryModelVisitor mongoDbEntityQueryModelVisitor,
            ExceptResultOperator exceptResultOperator)
            => HandleSetOperation(
                mongoDbEntityQueryModelVisitor,
                exceptResultOperator.Source2,
                mongoDbEntityQueryModelVisitor.QueryableMethodProvider.Except);

        private static Expression HandleFirst(
            MongoDbEntityQueryModelVisitor mongoDbEntityQueryModelVisitor,
            ChoiceResultOperatorBase choiceResultOperator)
            => CallWithPossibleCancellationToken(
                (choiceResultOperator.ReturnDefaultWhenEmpty
                    ? mongoDbEntityQueryModelVisitor.QueryableMethodProvider.FirstOrDefault
                    : mongoDbEntityQueryModelVisitor.QueryableMethodProvider.First)
                .MakeGenericMethod(mongoDbEntityQueryModelVisitor.Expression.Type.GetSequenceType()),
                mongoDbEntityQueryModelVisitor.Expression);

        private static Expression HandleGroup(
            MongoDbEntityQueryModelVisitor mongoDbEntityQueryModelVisitor,
            GroupResultOperator groupResultOperator,
            QueryModel queryModel)
        {
            var keySelector
                = mongoDbEntityQueryModelVisitor
                    .ReplaceClauseReferences(
                        groupResultOperator.KeySelector,
                        queryModel.MainFromClause);

            var elementSelector
                = mongoDbEntityQueryModelVisitor
                    .ReplaceClauseReferences(
                        groupResultOperator.ElementSelector,
                        queryModel.MainFromClause);

            var taskLiftingExpressionVisitor = new TaskLiftingExpressionVisitor();
            var asyncElementSelector = taskLiftingExpressionVisitor.LiftTasks(elementSelector);

            var expression
                = asyncElementSelector == elementSelector
                    ? Expression.Call(
                        mongoDbEntityQueryModelVisitor.QueryableMethodProvider.GroupBy
                            .MakeGenericMethod(
                                mongoDbEntityQueryModelVisitor.Expression.Type.GetSequenceType(),
                                keySelector.Type,
                                elementSelector.Type),
                        mongoDbEntityQueryModelVisitor.Expression,
                        Expression.Lambda(keySelector, mongoDbEntityQueryModelVisitor.CurrentParameter),
                        Expression.Lambda(elementSelector, mongoDbEntityQueryModelVisitor.CurrentParameter))
                    : Expression.Call(
                        _groupByAsync
                            .MakeGenericMethod(
                                mongoDbEntityQueryModelVisitor.Expression.Type.GetSequenceType(),
                                keySelector.Type,
                                elementSelector.Type),
                        mongoDbEntityQueryModelVisitor.Expression,
                        Expression.Lambda(keySelector, mongoDbEntityQueryModelVisitor.CurrentParameter),
                        Expression.Lambda(
                            asyncElementSelector,
                            mongoDbEntityQueryModelVisitor.CurrentParameter,
                            taskLiftingExpressionVisitor.CancellationTokenParameter));

            mongoDbEntityQueryModelVisitor.CurrentParameter
                = Expression.Parameter(expression.Type.GetSequenceType(), groupResultOperator.ItemName);

            mongoDbEntityQueryModelVisitor.QueryCompilationContext.AddOrUpdateMapping(groupResultOperator, mongoDbEntityQueryModelVisitor.CurrentParameter);

            return expression;
        }

        private static readonly MethodInfo _groupByAsync
            = typeof(ResultOperatorHandler)
                .GetTypeInfo()
                .GetDeclaredMethod(nameof(_GroupByAsync));

        // ReSharper disable once InconsistentNaming
        private static IAsyncEnumerable<IGrouping<TKey, TElement>> _GroupByAsync<TSource, TKey, TElement>(
            IAsyncEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, CancellationToken, Task<TElement>> elementSelector)
            => new AsyncGroupByAsyncEnumerable<TSource, TKey, TElement>(source, keySelector, elementSelector);

        private sealed class AsyncGroupByAsyncEnumerable<TSource, TKey, TElement>
            : IAsyncEnumerable<IGrouping<TKey, TElement>>
        {
            private readonly IAsyncEnumerable<TSource> _source;
            private readonly Func<TSource, TKey> _keySelector;
            private readonly Func<TSource, CancellationToken, Task<TElement>> _elementSelector;

            public AsyncGroupByAsyncEnumerable(
                IAsyncEnumerable<TSource> source,
                Func<TSource, TKey> keySelector,
                Func<TSource, CancellationToken, Task<TElement>> elementSelector)
            {
                _source = source;
                _keySelector = keySelector;
                _elementSelector = elementSelector;
            }

            public IAsyncEnumerator<IGrouping<TKey, TElement>> GetEnumerator()
                => new GroupByAsyncEnumerator(this);

            private sealed class GroupByAsyncEnumerator : IAsyncEnumerator<IGrouping<TKey, TElement>>
            {
                private readonly AsyncGroupByAsyncEnumerable<TSource, TKey, TElement> _groupByAsyncEnumerable;
                private readonly IEqualityComparer<TKey> _comparer;

                private IEnumerator<IGrouping<TKey, TSource>> _lookupEnumerator;
                private bool _hasNext;

                public GroupByAsyncEnumerator(
                    AsyncGroupByAsyncEnumerable<TSource, TKey, TElement> groupByAsyncEnumerable)
                {
                    _groupByAsyncEnumerable = groupByAsyncEnumerable;
                    _comparer = EqualityComparer<TKey>.Default;
                }

                public async Task<bool> MoveNext(CancellationToken cancellationToken)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (_lookupEnumerator == null)
                    {
                        _lookupEnumerator
                            = (await _groupByAsyncEnumerable._source
                                .ToLookup(
                                    _groupByAsyncEnumerable._keySelector,
                                    e => e,
                                    _comparer,
                                    cancellationToken)).GetEnumerator();

                        _hasNext = _lookupEnumerator.MoveNext();
                    }

                    if (_hasNext)
                    {
                        var grouping = new Grouping<TKey, TElement>(_lookupEnumerator.Current.Key);

                        foreach (var item in _lookupEnumerator.Current)
                        {
                            grouping.Add(await _groupByAsyncEnumerable._elementSelector(item, cancellationToken));
                        }

                        Current = grouping;

                        _hasNext = _lookupEnumerator.MoveNext();

                        return true;
                    }

                    return false;
                }

                public IGrouping<TKey, TElement> Current { get; private set; }

                public void Dispose() => _lookupEnumerator?.Dispose();
            }
        }

        private static Expression HandleIntersect(
            MongoDbEntityQueryModelVisitor mongoDbEntityQueryModelVisitor,
            IntersectResultOperator intersectResultOperator)
            => HandleSetOperation(
                mongoDbEntityQueryModelVisitor,
                intersectResultOperator.Source2,
                mongoDbEntityQueryModelVisitor.QueryableMethodProvider.Intersect);

        private static Expression HandleLast(
            MongoDbEntityQueryModelVisitor mongoDbEntityQueryModelVisitor,
            ChoiceResultOperatorBase choiceResultOperator)
        {
            if (mongoDbEntityQueryModelVisitor.Expression is MethodCallExpression methodCallExpression
                && methodCallExpression.Method
                    .MethodIsClosedFormOf(mongoDbEntityQueryModelVisitor.QueryableMethodProvider.Select))
            {
                // Push Last down below Select

                return
                    methodCallExpression.Update(
                        methodCallExpression.Object,
                        new[]
                        {
                            Expression.Call(
                                mongoDbEntityQueryModelVisitor.LinqOperatorProvider.ToSequence
                                    .MakeGenericMethod(methodCallExpression.Arguments[0].Type.GetSequenceType()),
                                Expression.Lambda(
                                    CallWithPossibleCancellationToken(
                                        (choiceResultOperator.ReturnDefaultWhenEmpty
                                            ? mongoDbEntityQueryModelVisitor.QueryableMethodProvider.LastOrDefault
                                            : mongoDbEntityQueryModelVisitor.QueryableMethodProvider.Last)
                                            .MakeGenericMethod(methodCallExpression.Arguments[0].Type.GetSequenceType()),
                                        methodCallExpression.Arguments[0]))),
                            methodCallExpression.Arguments[1]
                        });
            }

            return CallWithPossibleCancellationToken(
                (choiceResultOperator.ReturnDefaultWhenEmpty
                    ? mongoDbEntityQueryModelVisitor.QueryableMethodProvider.LastOrDefault
                    : mongoDbEntityQueryModelVisitor.QueryableMethodProvider.Last)
                .MakeGenericMethod(mongoDbEntityQueryModelVisitor.Expression.Type.GetSequenceType()),
                mongoDbEntityQueryModelVisitor.Expression);
        }

        private static Expression HandleLongCount(MongoDbEntityQueryModelVisitor mongoDbEntityQueryModelVisitor)
            => CallWithPossibleCancellationToken(
                mongoDbEntityQueryModelVisitor.QueryableMethodProvider.LongCount
                    .MakeGenericMethod(mongoDbEntityQueryModelVisitor.Expression.Type.GetSequenceType()),
                mongoDbEntityQueryModelVisitor.Expression);

        private static Expression HandleMin(MongoDbEntityQueryModelVisitor mongoDbEntityQueryModelVisitor)
            => HandleAggregate(mongoDbEntityQueryModelVisitor, "Min");

        private static Expression HandleMax(MongoDbEntityQueryModelVisitor mongoDbEntityQueryModelVisitor)
            => HandleAggregate(mongoDbEntityQueryModelVisitor, "Max");

        private static Expression HandleOfType(
            MongoDbEntityQueryModelVisitor mongoDbEntityQueryModelVisitor,
            OfTypeResultOperator ofTypeResultOperator)
            => Expression.Call(
                mongoDbEntityQueryModelVisitor.QueryableMethodProvider.OfType
                    .MakeGenericMethod(ofTypeResultOperator.SearchedItemType),
                mongoDbEntityQueryModelVisitor.Expression);

        private static Expression HandleSingle(
            MongoDbEntityQueryModelVisitor mongoDbEntityQueryModelVisitor,
            ChoiceResultOperatorBase choiceResultOperator)
            => CallWithPossibleCancellationToken(
                (choiceResultOperator.ReturnDefaultWhenEmpty
                    ? mongoDbEntityQueryModelVisitor.QueryableMethodProvider.SingleOrDefault
                    : mongoDbEntityQueryModelVisitor.QueryableMethodProvider.Single)
                .MakeGenericMethod(mongoDbEntityQueryModelVisitor.Expression.Type.GetSequenceType()),
                mongoDbEntityQueryModelVisitor.Expression);

        private static Expression HandleSkip(
            MongoDbEntityQueryModelVisitor mongoDbEntityQueryModelVisitor,
            SkipResultOperator skipResultOperator)
        {
            var countExpression
                = new DefaultQueryExpressionVisitor(mongoDbEntityQueryModelVisitor)
                    .Visit(skipResultOperator.Count);

            if (mongoDbEntityQueryModelVisitor.Expression is MethodCallExpression methodCallExpression
                && methodCallExpression.Method
                    .MethodIsClosedFormOf(mongoDbEntityQueryModelVisitor.QueryableMethodProvider.Select))
            {
                // Push Skip down below Select

                return
                    methodCallExpression.Update(
                        methodCallExpression.Object,
                        new[]
                        {
                            Expression.Call(
                                mongoDbEntityQueryModelVisitor.QueryableMethodProvider.Skip
                                    .MakeGenericMethod(methodCallExpression.Arguments[0].Type.GetSequenceType()),
                                methodCallExpression.Arguments[0],
                                countExpression),
                            methodCallExpression.Arguments[1]
                        });
            }

            return Expression.Call(
                mongoDbEntityQueryModelVisitor.QueryableMethodProvider.Skip
                    .MakeGenericMethod(mongoDbEntityQueryModelVisitor.Expression.Type.GetSequenceType()),
                mongoDbEntityQueryModelVisitor.Expression,
                countExpression);
        }

        private static Expression HandleSum(MongoDbEntityQueryModelVisitor mongoDbEntityQueryModelVisitor)
            => HandleAggregate(mongoDbEntityQueryModelVisitor, "Sum");

        private static Expression HandleTake(
            MongoDbEntityQueryModelVisitor mongoDbEntityQueryModelVisitor,
            TakeResultOperator takeResultOperator)
        {
            var countExpression
                = new DefaultQueryExpressionVisitor(mongoDbEntityQueryModelVisitor)
                    .Visit(takeResultOperator.Count);

            if (mongoDbEntityQueryModelVisitor.Expression is MethodCallExpression methodCallExpression
                && methodCallExpression.Method
                    .MethodIsClosedFormOf(mongoDbEntityQueryModelVisitor.QueryableMethodProvider.Select))
            {
                // Push Take down below Select

                return
                    methodCallExpression.Update(
                        methodCallExpression.Object,
                        new[]
                        {
                            Expression.Call(
                                mongoDbEntityQueryModelVisitor.QueryableMethodProvider.Take
                                    .MakeGenericMethod(methodCallExpression.Arguments[0].Type.GetSequenceType()),
                                methodCallExpression.Arguments[0],
                                countExpression),
                            methodCallExpression.Arguments[1]
                        });
            }

            return Expression.Call(
                mongoDbEntityQueryModelVisitor.QueryableMethodProvider.Take
                    .MakeGenericMethod(mongoDbEntityQueryModelVisitor.Expression.Type.GetSequenceType()),
                mongoDbEntityQueryModelVisitor.Expression,
                countExpression);
        }

        private static Expression HandleUnion(
            MongoDbEntityQueryModelVisitor mongoDbEntityQueryModelVisitor,
            UnionResultOperator unionResultOperator)
            => HandleSetOperation(
                mongoDbEntityQueryModelVisitor,
                unionResultOperator.Source2,
                mongoDbEntityQueryModelVisitor.QueryableMethodProvider.Union);

        private static Expression HandleSetOperation(
            MongoDbEntityQueryModelVisitor mongoDbEntityQueryModelVisitor,
            Expression secondSource,
            MethodInfo setMethodInfo)
        {
            var source2 = mongoDbEntityQueryModelVisitor
                .ReplaceClauseReferences(secondSource);

            var resultType = mongoDbEntityQueryModelVisitor.Expression.Type.GetSequenceType();
            var sourceType = source2.Type.GetSequenceType();
            while (!resultType.GetTypeInfo().IsAssignableFrom(sourceType.GetTypeInfo()))
            {
                resultType = resultType.GetTypeInfo().BaseType;
            }

            return Expression.Call(
                setMethodInfo.MakeGenericMethod(resultType),
                mongoDbEntityQueryModelVisitor.Expression,
                source2);
        }

        private static Expression HandleAggregate(
            MongoDbEntityQueryModelVisitor mongoDbEntityQueryModelVisitor,
            string methodName)
            => CallWithPossibleCancellationToken(
                mongoDbEntityQueryModelVisitor.QueryableMethodProvider.GetAggregateMethod(
                    methodName,
                    mongoDbEntityQueryModelVisitor.Expression.Type.GetSequenceType()),
                mongoDbEntityQueryModelVisitor.Expression);

        private static readonly PropertyInfo _cancellationTokenProperty
            = typeof(QueryContext).GetTypeInfo()
                .GetDeclaredProperty("CancellationToken");
    }
}
