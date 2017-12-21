using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query
{
    /// <inheritdoc />
    public class MongoDbLinqOperatorProvider : LinqOperatorProvider, IMongoDbLinqOperatorProvider
    {
        private static readonly MethodInfo AllMethodInfo = MethodHelper.GetGenericMethodDefinition(
            () => Enumerable.All<object>(null, null));

        private static readonly MethodInfo AnyMethodInfo = MethodHelper.GetGenericMethodDefinition(
            () => Enumerable.Any<object>(null));

        private static readonly MethodInfo CastMethodInfo = MethodHelper.GetGenericMethodDefinition(
            () => Enumerable.Cast<object>(null));

        private static readonly MethodInfo ConcatMethodInfo = MethodHelper.GetGenericMethodDefinition(
            () => Enumerable.Concat<object>(null, null));

        private static readonly MethodInfo ContainsMethodInfo = MethodHelper.GetGenericMethodDefinition(
            () => Enumerable.Contains<object>(null, null));

        private static readonly MethodInfo CountMethodInfo = MethodHelper.GetGenericMethodDefinition(
            () => Enumerable.Count<object>(null, null));

        //private static readonly MethodInfo DefaultIfEmptyMethodInfo = MethodHelper.GetMethodInfo(
        //    () => Enumerable.DefaultIfEmpty<object>(null));

        //private static readonly MethodInfo DefaultIfEmptyArgMethodInfo = MethodHelper.GetMethodInfo(
        //    () => Enumerable.DefaultIfEmpty<object>(null, null));

        private static readonly MethodInfo DistinctMethodInfo = MethodHelper.GetGenericMethodDefinition(
            () => Enumerable.Distinct<object>(null));

        private static readonly MethodInfo ExceptMethodInfo = MethodHelper.GetGenericMethodDefinition(
            () => Enumerable.Except<object>(null, null));

        private static readonly MethodInfo FirstMethodInfo = MethodHelper.GetGenericMethodDefinition(
            () => Enumerable.First<object>(null));

        private static readonly MethodInfo FirstOrDefaultMethodInfo = MethodHelper.GetGenericMethodDefinition(
            () => Enumerable.FirstOrDefault<object>(null));

        private static readonly MethodInfo GroupByMethodInfo = MethodHelper.GetGenericMethodDefinition(
            () => Enumerable.GroupBy<object, object, object, object>(null, null, null, null, null));

        private static readonly MethodInfo GroupJoinMethodInfo = MethodHelper.GetGenericMethodDefinition(
            () => Enumerable.GroupJoin<object, object, object, object>(null, null, null, null, null));

        private static readonly MethodInfo IntersectMethodInfo = MethodHelper.GetGenericMethodDefinition(
            () => Enumerable.Intersect<object>(null, null));

        private static readonly MethodInfo JoinMethodInfo = MethodHelper.GetGenericMethodDefinition(
            () => Enumerable.Join<object, object, object, object>(null, null, null, null, null));

        private static readonly MethodInfo LastMethodInfo = MethodHelper.GetGenericMethodDefinition(
            () => Enumerable.Last<object>(null));

        private static readonly MethodInfo LastOrDefaultMethodInfo = MethodHelper.GetGenericMethodDefinition(
            () => Enumerable.LastOrDefault<object>(null));

        private static readonly MethodInfo LongCountMethodInfo = MethodHelper.GetGenericMethodDefinition(
            () => Enumerable.LongCount<object>(null, null));

        private static readonly MethodInfo OfTypeMethodInfo = MethodHelper.GetGenericMethodDefinition(
            () => Enumerable.OfType<object>(null));

        private static readonly MethodInfo OrderByMethodInfo = MethodHelper.GetGenericMethodDefinition(
            () => Enumerable.OrderBy<object, object>(null, null));

        private static readonly MethodInfo OrderByDescendingMethodInfo = MethodHelper.GetGenericMethodDefinition(
            () => Enumerable.OrderByDescending<object, object>(null, null));

        private static readonly MethodInfo SelectMethodInfo = MethodHelper.GetGenericMethodDefinition(
            () => Enumerable.Select(null, (Func<object, object>) null));

        private static readonly MethodInfo SelectManyMethodInfo = MethodHelper.GetGenericMethodDefinition(
            () => Enumerable.SelectMany<object, object, object>(null, (Func<object, IEnumerable<object>>) null, null));

        private static readonly MethodInfo SingleMethodInfo = MethodHelper.GetGenericMethodDefinition(
            () => Enumerable.Single<object>(null));

        private static readonly MethodInfo SingleOrDefaultMethodInfo = MethodHelper.GetGenericMethodDefinition(
            () => Enumerable.SingleOrDefault<object>(null));

        private static readonly MethodInfo SkipMethodInfo = MethodHelper.GetGenericMethodDefinition(
            () => Enumerable.Skip<object>(null, 0));

        private static readonly MethodInfo TakeMethodInfo = MethodHelper.GetGenericMethodDefinition(
            () => Enumerable.Take<object>(null, 0));

        private static readonly MethodInfo ThenByMethodInfo = MethodHelper.GetGenericMethodDefinition(
            () => Enumerable.ThenBy<object, object>(null, null));

        private static readonly MethodInfo ThenByDescendingMethodInfo = MethodHelper.GetGenericMethodDefinition(
            () => Enumerable.ThenByDescending<object, object>(null, null));

        private static readonly MethodInfo ToListMethodInfo = MethodHelper.GetGenericMethodDefinition(
            () => Enumerable.ToList<object>(null));

        private static readonly MethodInfo TrackEntitiesMethodInfo = MethodHelper.GetGenericMethodDefinition(
            () => _TrackEntities<object, object>(null, null, null, null));

        private static readonly MethodInfo UnionMethodInfo = MethodHelper.GetGenericMethodDefinition(
            () => Enumerable.Union<object>(null, null));

        private static readonly MethodInfo WhereMethodInfo = MethodHelper.GetGenericMethodDefinition(
            () => Enumerable.Where(null, (Func<object, bool>) null));

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

        ///// <inheritdoc />
        //public override MethodInfo DefaultIfEmpty => DefaultIfEmptyMethodInfo;

        ///// <inheritdoc />
        //public override MethodInfo DefaultIfEmptyArg => DefaultIfEmptyArgMethodInfo;

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
        public override MethodInfo Intersect => IntersectMethodInfo;

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
        public virtual MethodInfo OrderByDescending => OrderByDescendingMethodInfo;

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
        public override MethodInfo ThenBy => ThenByMethodInfo;

        /// <inheritdoc />
        public virtual MethodInfo ThenByDescending => ThenByDescendingMethodInfo;

        /// <inheritdoc />
        public virtual MethodInfo ToList => ToListMethodInfo;

        /// <inheritdoc />
        public override MethodInfo TrackEntities => TrackEntitiesMethodInfo;

        /// <inheritdoc />
        public override MethodInfo Union => UnionMethodInfo;

        /// <inheritdoc />
        public override MethodInfo Where => WhereMethodInfo;

        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        private static IEnumerable<TOut> _TrackEntities<TOut, TIn>(
            IEnumerable<TOut> results,
            QueryContext queryContext,
            IList<EntityTrackingInfo> entityTrackingInfos,
            IList<Func<TIn, object>> entityAccessors)
            where TIn : class
        {
            queryContext.BeginTrackingQuery();

            foreach (var result in results)
            {
                if (result != null)
                {
                    for (var i = 0; i < entityTrackingInfos.Count; i++)
                    {
                        var entityOrCollection = entityAccessors[i](result as TIn);

                        if (entityOrCollection != null)
                        {
                            var entityTrackingInfo = entityTrackingInfos[i];

                            if (entityTrackingInfo.IsEnumerableTarget)
                            {
                                foreach (var entity in (IEnumerable)entityOrCollection)
                                {
                                    queryContext.StartTracking(entity, entityTrackingInfos[i]);
                                }
                            }
                            else
                            {
                                queryContext.StartTracking(entityOrCollection, entityTrackingInfos[i]);
                            }
                        }
                    }
                }

                yield return result;
            }
        }
    }
}
