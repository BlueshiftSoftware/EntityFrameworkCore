using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query
{
    /// <inheritdoc cref="LinqOperatorProvider" />
    /// <inheritdoc cref="IQueryableLinqOperatorProvider" />
    public class MongoQueryableLinqOperatorProvider : LinqOperatorProvider, IQueryableLinqOperatorProvider
    {
        private static readonly MethodInfo OrderByMethodInfo =
            MethodHelper.GetGenericMethodDefinition<IEnumerable<object>, IOrderedEnumerable<object>>(
                enumerable => enumerable.OrderBy<object, object>(obj => null));

        private static readonly MethodInfo OrderByDescendingMethodInfo =
            MethodHelper.GetGenericMethodDefinition<IOrderedEnumerable<object>, IOrderedEnumerable<object>>(
                enumerable => enumerable.OrderByDescending<object, object>(obj => null));

        private static readonly MethodInfo ThenByMethodInfo =
            MethodHelper.GetGenericMethodDefinition<IOrderedEnumerable<object>, IOrderedEnumerable<object>>(
                orderedEnumerable => orderedEnumerable.ThenBy<object, object>(obj => null));

        private static readonly MethodInfo ThenByDescendingMethodInfo =
            MethodHelper.GetGenericMethodDefinition<IOrderedEnumerable<object>, IOrderedEnumerable<object>>(
                orderedEnumerable => orderedEnumerable.ThenByDescending<object, object>(obj => null));

        /// <inheritdoc />
        public override MethodInfo OrderBy => OrderByMethodInfo;

        /// <inheritdoc />
        public virtual MethodInfo OrderByDescending => OrderByDescendingMethodInfo;

        /// <inheritdoc />
        public override MethodInfo ThenBy => ThenByMethodInfo;

        /// <inheritdoc />
        public virtual MethodInfo ThenByDescending => ThenByDescendingMethodInfo;

    }
}
