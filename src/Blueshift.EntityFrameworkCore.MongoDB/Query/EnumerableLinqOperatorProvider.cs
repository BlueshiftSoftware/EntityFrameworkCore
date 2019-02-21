using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query
{
    /// <inheritdoc />
    public class EnumerableLinqOperatorProvider : LinqOperatorProvider
    {
        private static readonly MethodInfo SelectMethodInfo =
            MethodHelper.GetGenericMethodDefinition<IEnumerable<object>, IEnumerable<object>>(
                enumerable => enumerable.Select(obj => obj));

        private static readonly MethodInfo SelectManyMethodInfo =
            MethodHelper.GetGenericMethodDefinition<IEnumerable<object>, IEnumerable<object>>(
                enumerable => enumerable.SelectMany<object, object, object>(
                    obj => null,
                    (obj1, obj2) => null));

        private static readonly MethodInfo WhereMethodInfo =
            MethodHelper.GetGenericMethodDefinition<IEnumerable<object>, IEnumerable<object>>(
                enumerable => enumerable.Where(obj => false));

        /// <inheritdoc />
        public override MethodInfo Select => SelectMethodInfo;

        /// <inheritdoc />
        public override MethodInfo SelectMany => SelectManyMethodInfo;

        /// <inheritdoc />
        public override MethodInfo Where => WhereMethodInfo;
    }
}
