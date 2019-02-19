using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Driver.Linq;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query
{
    /// <inheritdoc />
    public class QueryableMethodProvider : IQueryableMethodProvider
    {
        private static readonly MethodInfo AllMethodInfo =
            MethodHelper.GetGenericMethodDefinition<IQueryable<object>, bool>(
                queryable => queryable.All(obj => false));

        private static readonly MethodInfo AnyMethodInfo =
            MethodHelper.GetGenericMethodDefinition<IQueryable<object>, bool>(
                queryable => queryable.Any(obj => false));

        private static readonly MethodInfo CastMethodInfo =
            MethodHelper.GetGenericMethodDefinition<IQueryable<object>, IQueryable<object>>(
                queryable => queryable.Cast<object>());

        private static readonly MethodInfo ConcatMethodInfo =
            MethodHelper.GetGenericMethodDefinition<IQueryable<object>, IQueryable<object>>(
                queryable => queryable.Concat(null));

        private static readonly MethodInfo ContainsMethodInfo =
            MethodHelper.GetGenericMethodDefinition<IQueryable<object>, bool>(
                queryable => queryable.Contains(null));

        private static readonly MethodInfo CountMethodInfo =
            MethodHelper.GetGenericMethodDefinition<IQueryable<object>, int>(
                queryable => queryable.Count());

        private static readonly MethodInfo DefaultIfEmptyMethodInfo =
            MethodHelper.GetGenericMethodDefinition<IQueryable<object>, IQueryable<object>>(
                queryable => queryable.DefaultIfEmpty());

        private static readonly MethodInfo DefaultIfEmptyParameterMethodInfo =
            MethodHelper.GetGenericMethodDefinition<IQueryable<object>, IQueryable<object>>(
                queryable => queryable.DefaultIfEmpty(null));

        private static readonly MethodInfo DistinctMethodInfo =
            MethodHelper.GetGenericMethodDefinition<IQueryable<object>, IQueryable<object>>(
                queryable => queryable.Distinct());

        private static readonly MethodInfo ExceptMethodInfo =
            MethodHelper.GetGenericMethodDefinition<IQueryable<object>, IQueryable<object>>(
                queryable => queryable.Except(null));

        private static readonly MethodInfo FirstMethodInfo =
            MethodHelper.GetGenericMethodDefinition<IQueryable<object>, object>(
                queryable => queryable.First());

        private static readonly MethodInfo FirstOrDefaultMethodInfo =
            MethodHelper.GetGenericMethodDefinition<IQueryable<object>, object>(
                queryable => queryable.FirstOrDefault());

        private static readonly MethodInfo GroupJoinMethodInfo =
            MethodHelper.GetGenericMethodDefinition<IQueryable<object>, IQueryable<object>>(
                queryable => queryable.GroupJoin<object, object, object, object>(
                    null,
                    obj => null,
                    obj => null,
                    (obj1, obj2) => null));

        private static readonly MethodInfo GroupByMethodInfo =
            MethodHelper.GetGenericMethodDefinition<IQueryable<object>, IQueryable<IGrouping<object, object>>>(
                queryable => queryable.GroupBy<object, object, object>(
                    obj => null,
                    obj => null));

        private static readonly MethodInfo IntersectMethodInfo =
            MethodHelper.GetGenericMethodDefinition<IQueryable<object>, IQueryable<object>>(
                queryable => queryable.Intersect(null));

        private static readonly MethodInfo JoinMethodInfo =
            MethodHelper.GetGenericMethodDefinition<IQueryable<object>, IQueryable<object>>(
                queryable => queryable.Join<object, object, object, object>(
                    null,
                    obj => null,
                    obj => null,
                    (obj1, obj2) => null));

        private static readonly MethodInfo LastMethodInfo =
            MethodHelper.GetGenericMethodDefinition<IQueryable<object>, object>(
                queryable => queryable.Last());

        private static readonly MethodInfo LastOrDefaultMethodInfo =
            MethodHelper.GetGenericMethodDefinition<IQueryable<object>, object>(
                queryable => queryable.LastOrDefault());

        private static readonly MethodInfo LongCountMethodInfo =
            MethodHelper.GetGenericMethodDefinition<IQueryable<object>, long>(
                queryable => queryable.LongCount());

        private static readonly MethodInfo OfTypeMethodInfo =
            MethodHelper.GetGenericMethodDefinition<IQueryable<object>, IQueryable<object>>(
                queryable => queryable.OfType<object>());

        private static readonly MethodInfo OrderByMethodInfo =
            MethodHelper.GetGenericMethodDefinition<IQueryable<object>, IQueryable<object>>(
                queryable => queryable.OrderBy<object, object>(obj => null));

        private static readonly MethodInfo OrderByDescendingMethodInfo =
            MethodHelper.GetGenericMethodDefinition<IQueryable<object>, IQueryable<object>>(
                queryable => queryable.OrderByDescending<object, object>(obj => null));

        private static readonly MethodInfo SelectMethodInfo =
            MethodHelper.GetGenericMethodDefinition<IQueryable<object>, IQueryable<object>>(
                queryable => queryable.Select<object, object>(obj => null));

        private static readonly MethodInfo SelectManyMethodInfo =
            MethodHelper.GetGenericMethodDefinition<IQueryable<object>, IQueryable<object>>(
                queryable => queryable.SelectMany<object, object, object>(
                    obj => null,
                    (obj1, obj2) => null));

        private static readonly MethodInfo SingleMethodInfo =
            MethodHelper.GetGenericMethodDefinition<IQueryable<object>, object>(
                queryable => queryable.Single());

        private static readonly MethodInfo SingleOrDefaultMethodInfo =
            MethodHelper.GetGenericMethodDefinition<IQueryable<object>, object>(
                queryable => queryable.SingleOrDefault());

        private static readonly MethodInfo SkipMethodInfo =
            MethodHelper.GetGenericMethodDefinition<IQueryable<object>, IQueryable<object>>(
                queryable => queryable.Skip(0));

        private static readonly MethodInfo TakeMethodInfo =
            MethodHelper.GetGenericMethodDefinition<IQueryable<object>, IQueryable<object>>(
                queryable => queryable.Take(0));

        private static readonly MethodInfo ThenByMethodInfo =
            MethodHelper.GetGenericMethodDefinition<IOrderedQueryable<object>, IOrderedQueryable<object>>(
                orderedQueryable => orderedQueryable.ThenBy<object, object>(obj => null));

        private static readonly MethodInfo ThenByDescendingMethodInfo =
            MethodHelper.GetGenericMethodDefinition<IOrderedQueryable<object>, IOrderedQueryable<object>>(
                orderedQueryable => orderedQueryable.ThenByDescending<object, object>(obj => null));

        private static readonly MethodInfo UnionMethodInfo =
            MethodHelper.GetGenericMethodDefinition<IQueryable<object>, IQueryable<object>>(
                queryable => queryable.Union(null));

        private static readonly MethodInfo WhereMethodInfo =
            MethodHelper.GetGenericMethodDefinition<IQueryable<object>, IQueryable<object>>(
                queryable => queryable.Where(obj => false));

        /// <inheritdoc />
        public virtual MethodInfo All => AllMethodInfo;

        /// <inheritdoc />
        public virtual MethodInfo Any => AnyMethodInfo;

        /// <inheritdoc />
        public virtual MethodInfo Cast => CastMethodInfo;

        /// <inheritdoc />
        public virtual MethodInfo Concat => ConcatMethodInfo;

        /// <inheritdoc />
        public virtual MethodInfo Contains => ContainsMethodInfo;

        /// <inheritdoc />
        public virtual MethodInfo Count => CountMethodInfo;

        /// <inheritdoc />
        public virtual MethodInfo DefaultIfEmpty => DefaultIfEmptyMethodInfo;

        /// <inheritdoc />
        public virtual MethodInfo DefaultIfEmptyParameter => DefaultIfEmptyParameterMethodInfo;

        /// <inheritdoc />
        public virtual MethodInfo Distinct => DistinctMethodInfo;

        /// <inheritdoc />
        public virtual MethodInfo Except => ExceptMethodInfo;

        /// <inheritdoc />
        public virtual MethodInfo First => FirstMethodInfo;

        /// <inheritdoc />
        public virtual MethodInfo FirstOrDefault => FirstOrDefaultMethodInfo;

        /// <inheritdoc />
        public virtual MethodInfo GroupJoin => GroupJoinMethodInfo;

        /// <inheritdoc />
        public virtual MethodInfo GroupBy => GroupByMethodInfo;

        /// <inheritdoc />
        public virtual MethodInfo Intersect => IntersectMethodInfo;

        /// <inheritdoc />
        public virtual MethodInfo Join => JoinMethodInfo;

        /// <inheritdoc />
        public virtual MethodInfo Last => LastMethodInfo;

        /// <inheritdoc />
        public virtual MethodInfo LastOrDefault => LastOrDefaultMethodInfo;

        /// <inheritdoc />
        public virtual MethodInfo LongCount => LongCountMethodInfo;

        /// <inheritdoc />
        public virtual MethodInfo OfType => OfTypeMethodInfo;

        /// <inheritdoc />
        public virtual MethodInfo OrderBy => OrderByMethodInfo;

        /// <inheritdoc />
        public virtual MethodInfo OrderByDescending => OrderByDescendingMethodInfo;

        /// <inheritdoc />
        public virtual MethodInfo Select => SelectMethodInfo;

        /// <inheritdoc />
        public virtual MethodInfo SelectMany => SelectManyMethodInfo;

        /// <inheritdoc />
        public virtual MethodInfo Single => SingleMethodInfo;

        /// <inheritdoc />
        public virtual MethodInfo SingleOrDefault => SingleOrDefaultMethodInfo;

        /// <inheritdoc />
        public virtual MethodInfo Skip => SkipMethodInfo;

        /// <inheritdoc />
        public virtual MethodInfo Take => TakeMethodInfo;

        /// <inheritdoc />
        public virtual MethodInfo ThenBy => ThenByMethodInfo;

        /// <inheritdoc />
        public virtual MethodInfo ThenByDescending => ThenByDescendingMethodInfo;

        /// <inheritdoc />
        public virtual MethodInfo Union => UnionMethodInfo;

        /// <inheritdoc />
        public virtual MethodInfo Where => WhereMethodInfo;

        /// <inheritdoc />
        public bool IsQueryableExpression(Expression expression)
            => expression.Type.TryGetImplementationType(typeof(IQueryable<>), out Type queryableType);

        /// <inheritdoc />
        public MethodInfo GetAggregateMethod(string methodName, Type elementType)
        {
            Check.NotEmpty(methodName, nameof(methodName));
            Check.NotNull(elementType, nameof(elementType));

            var aggregateMethods = GetMethods(methodName).ToList();

            return
                aggregateMethods
                    .FirstOrDefault(
                        mi => mi.GetParameters()[0].ParameterType
                              == typeof(IQueryable<>).MakeGenericType(elementType))
                ?? aggregateMethods.Single(mi => mi.IsGenericMethod)
                    .MakeGenericMethod(elementType);
        }

        private static IEnumerable<MethodInfo> GetMethods(string name, int parameterCount = 0)
            => typeof(Queryable).GetTypeInfo().GetDeclaredMethods(name)
                .Where(mi => mi.GetParameters().Length == parameterCount + 1);

        private static readonly MethodInfo LambdaMethodInfo =
            MethodHelper.GetGenericMethodDefinition<Expression<Func<object>>>(
                () => Expression.Lambda<Func<object>>(null, (ParameterExpression[]) null));

        /// <inheritdoc />
        public Expression CreateLambdaExpression(
            Expression bodyExpression,
            params ParameterExpression[] parameterExpressions)
        {
            Type[] delegateGenericTypeArgs = parameterExpressions
                .Concat(new[] {bodyExpression})
                .Select(expression =>
                    expression.Type.TryGetImplementationType(typeof(IQueryable<>), out Type queryableType)
                        ? queryableType
                        : expression.Type != typeof(string)
                          && expression.Type != typeof(byte[])
                          && expression.Type.TryGetImplementationType(typeof(IEnumerable<>), out Type enumerableType)
                            ? enumerableType
                            : expression.Type)
                .ToArray();
            Type delegateType = Expression.GetFuncType(delegateGenericTypeArgs);

            LambdaExpression lambdaExpression = (LambdaExpression) LambdaMethodInfo
                .MakeGenericMethod(delegateType)
                .Invoke(
                    null,
                    new object[]
                    {
                        bodyExpression,
                        parameterExpressions
                    });
            return lambdaExpression;
        }
    }
}
