using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Blueshift.EntityFrameworkCore.Query
{
    public class MongoDbQueryContext : QueryContext
    {
        public MongoDbQueryContext(
            [NotNull] QueryContextDependencies queryContextDependencies,
            [NotNull] Func<IQueryBuffer> queryBufferFactory)
            : base(queryContextDependencies, queryBufferFactory)
        {
        }
    }
}