using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query
{
    /// <inheritdoc />
    public class QueryableLinqOperatorProvider : LinqOperatorProvider
    {
        private static readonly MethodInfo AllMethodInfo = MethodHelper.GetGenericMethodDefinition<object>(
            () => Queryable.All<object>(null, obj => false));

        private static readonly MethodInfo AnyMethodInfo = MethodHelper.GetGenericMethodDefinition<object>(
            () => Queryable.Any<object>(null, obj => false));

        private static readonly MethodInfo CastMethodInfo = MethodHelper.GetGenericMethodDefinition<object>(
            () => Queryable.Cast<object>(null));

        private static readonly MethodInfo ConcatMethodInfo = MethodHelper.GetGenericMethodDefinition<object>(
            () => Queryable.Concat<object>(null, null));

        private static readonly MethodInfo ContainsMethodInfo = MethodHelper.GetGenericMethodDefinition<object>(
            () => Queryable.Contains<object>(null, null));

        private static readonly MethodInfo CountMethodInfo = MethodHelper.GetGenericMethodDefinition<object>(
            () => Queryable.Count<object>(null));

        private static readonly MethodInfo DefaultIfEmptyMethodInfo = MethodHelper.GetGenericMethodDefinition<object>(
            () => Queryable.DefaultIfEmpty<object>(null));

        private static readonly MethodInfo ParameterizedDefaultIfEmptyMethodInfo = MethodHelper.GetGenericMethodDefinition<object>(
            () => Queryable.DefaultIfEmpty<object>(null, null));

        private static readonly MethodInfo DistinctMethodInfo = MethodHelper.GetGenericMethodDefinition<object>(
            () => Queryable.Distinct<object>(null));

        private static readonly MethodInfo ExceptMethodInfo = MethodHelper.GetGenericMethodDefinition<object>(
            () => Queryable.Except<object>(null, null));

        private static readonly MethodInfo FirstMethodInfo = MethodHelper.GetGenericMethodDefinition<object>(
            () => Queryable.First<object>(null, obj => false));

        private static readonly MethodInfo FirstOrDefaultMethodInfo = MethodHelper.GetGenericMethodDefinition<object>(
            () => Queryable.FirstOrDefault<object>(null, obj => false));

        private static readonly MethodInfo GroupByMethodInfo = MethodHelper.GetGenericMethodDefinition<object>(
            () => Queryable.GroupBy<object, object, object>(
                null,
                obj => null,
                (obj1, obj2) => null));

        private static readonly MethodInfo GroupJoinMethodInfo = MethodHelper.GetGenericMethodDefinition<object>(
            () => Queryable.GroupJoin<object, object, object, object>(
                null,
                null,
                obj => null,
                obj => null,
                (@obj1, @obj2) => null));

        private static readonly MethodInfo IntersectByMethodInfo = MethodHelper.GetGenericMethodDefinition<object>(
            () => Queryable.Intersect<object>(null, null));

        private static readonly MethodInfo JoinMethodInfo = MethodHelper.GetGenericMethodDefinition<object>(
            () => Queryable.Join<object, object, object, object>(
                null,
                null,
                obj => null,
                obj => null,
                (@obj1, @obj2) => null));

        private static readonly MethodInfo LastMethodInfo = MethodHelper.GetGenericMethodDefinition<object>(
            () => Queryable.Last<object>(null, obj => false));

        private static readonly MethodInfo LastOrDefaultMethodInfo = MethodHelper.GetGenericMethodDefinition<object>(
            () => Queryable.LastOrDefault<object>(null, obj => false));

        private static readonly MethodInfo LongCountMethodInfo = MethodHelper.GetGenericMethodDefinition<object>(
            () => Queryable.LongCount<object>(null, obj => false));

        private static readonly MethodInfo OfTypeMethodInfo = MethodHelper.GetGenericMethodDefinition<object>(
            () => Queryable.OfType<object>(null));

        private static readonly MethodInfo OrderByMethodInfo = MethodHelper.GetGenericMethodDefinition<object>(
            () => Queryable.OrderBy<object, object>(null, obj => null));

        private static readonly MethodInfo SelectMethodInfo = MethodHelper.GetGenericMethodDefinition<object>(
            () => Queryable.Select<object, object>(null, obj => null));

        private static readonly MethodInfo SelectManyMethodInfo = MethodHelper.GetGenericMethodDefinition<object>(
            () => Queryable.SelectMany<object, object>(null, obj => null));

        private static readonly MethodInfo SingleMethodInfo = MethodHelper.GetGenericMethodDefinition<object>(
            () => Queryable.Single<object>(null, obj => false));

        private static readonly MethodInfo SingleOrDefaultMethodInfo = MethodHelper.GetGenericMethodDefinition<object>(
            () => Queryable.SingleOrDefault<object>(null, obj => false));

        private static readonly MethodInfo SkipMethodInfo = MethodHelper.GetGenericMethodDefinition<object>(
            () => Queryable.Skip<object>(null, 0));

        private static readonly MethodInfo TakeMethodInfo = MethodHelper.GetGenericMethodDefinition<object>(
            () => Queryable.Take<object>(null, 0));

        private static readonly MethodInfo UnionMethodInfo = MethodHelper.GetGenericMethodDefinition<object>(
            () => Queryable.Union<object>(null, null));

        private static readonly MethodInfo WhereMethodInfo = MethodHelper.GetGenericMethodDefinition<object>(
            () => Queryable.Where<object>(null, obj => false));

        /// <inheritdoc />
        public override MethodInfo GetAggregateMethod(string methodName, Type elementType)
            => base.GetAggregateMethod(methodName, elementType);

        /// <inheritdoc />
        public override Type MakeSequenceType(Type elementType)
            => base.MakeSequenceType(elementType);

        /// <inheritdoc />
        public override MethodInfo All => AllMethodInfo;

        /// <inheritdoc />
        public override MethodInfo Any => AnyMethodInfo;

        /// <inheritdoc />
        public override MethodInfo Cast => CastMethodInfo;

        /// <inheritdoc />
        public override MethodInfo Concat => ConcatMethodInfo;

        /// <inheritdoc />
        public override MethodInfo Contains => ContainsMethodInfo;

        /// <inheritdoc />
        public override MethodInfo Count => CountMethodInfo;

        /// <inheritdoc />
        public override MethodInfo DefaultIfEmpty => DefaultIfEmptyMethodInfo;

        /// <inheritdoc />
        public override MethodInfo DefaultIfEmptyArg => ParameterizedDefaultIfEmptyMethodInfo;

        /// <inheritdoc />
        public override MethodInfo Distinct => DistinctMethodInfo;

        /// <inheritdoc />
        public override MethodInfo Except => ExceptMethodInfo;

        /// <inheritdoc />
        public override MethodInfo First => FirstMethodInfo;

        /// <inheritdoc />
        public override MethodInfo FirstOrDefault => FirstOrDefaultMethodInfo;

        /// <inheritdoc />
        public override MethodInfo GroupBy => GroupByMethodInfo;

        /// <inheritdoc />
        public override MethodInfo GroupJoin => GroupJoinMethodInfo;

        /// <inheritdoc />
        public override MethodInfo Intersect => IntersectByMethodInfo;

        /// <inheritdoc />
        public override MethodInfo Join => JoinMethodInfo;

        /// <inheritdoc />
        public override MethodInfo Last => LastMethodInfo;

        /// <inheritdoc />
        public override MethodInfo LastOrDefault => LastOrDefaultMethodInfo;

        /// <inheritdoc />
        public override MethodInfo LongCount => LongCountMethodInfo;

        /// <inheritdoc />
        public override MethodInfo OfType => OfTypeMethodInfo;

        /// <inheritdoc />
        public override MethodInfo OrderBy => OrderByMethodInfo;

        /// <inheritdoc />
        public override MethodInfo Select => SelectMethodInfo;

        /// <inheritdoc />
        public override MethodInfo SelectMany => SelectManyMethodInfo;

        /// <inheritdoc />
        public override MethodInfo Single => SingleMethodInfo;

        /// <inheritdoc />
        public override MethodInfo SingleOrDefault => SingleOrDefaultMethodInfo;

        /// <inheritdoc />
        public override MethodInfo Skip => SkipMethodInfo;

        /// <inheritdoc />
        public override MethodInfo Take => TakeMethodInfo;

        /// <inheritdoc />
        public override MethodInfo ThenBy => TakeMethodInfo;

        /// <inheritdoc />
        public override MethodInfo Union => UnionMethodInfo;

        /// <inheritdoc />
        public override MethodInfo Where => WhereMethodInfo;

        /// <inheritdoc />
        public override MethodInfo ToSequence => base.ToSequence;

        /// <inheritdoc />
        public override MethodInfo ToOrdered => base.ToOrdered;

        /// <inheritdoc />
        public override MethodInfo ToEnumerable => base.ToEnumerable;

        /// <inheritdoc />
        public override MethodInfo ToQueryable => base.ToQueryable;
    }
}
