using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Blueshift.EntityFrameworkCore.Query
{
    /// <summary>
    ///     The principal data structure used by a compiled query during execution.
    /// </summary>
    public class MongoDbQueryContext : QueryContext
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended
        ///     to be used directly from your code. This API may change or be removed in future
        ///     releases.
        /// </summary>
        public MongoDbQueryContext(
            [NotNull] QueryContextDependencies queryContextDependencies,
            [NotNull] Func<IQueryBuffer> queryBufferFactory)
            : base(queryContextDependencies, queryBufferFactory)
        {
        }
    }
}