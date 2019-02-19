using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query
{
    /// <inheritdoc />
    public class EnumerableLinqOperatorProvider : LinqOperatorProvider
    {
        private static readonly MethodInfo WhereMethodInfo =
            MethodHelper.GetGenericMethodDefinition<IEnumerable<object>, IEnumerable<object>>(
                enumerable => enumerable.Where(obj => false));

        /// <inheritdoc />
        public override MethodInfo Where => WhereMethodInfo;
    }
}
