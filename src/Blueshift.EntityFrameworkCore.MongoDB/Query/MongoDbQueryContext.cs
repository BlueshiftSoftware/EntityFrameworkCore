using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query
{
    /// <inheritdoc />
    public class MongoDbQueryContext : QueryContext
    {
        /// <inheritdoc />
        public MongoDbQueryContext(
            [NotNull] QueryContextDependencies queryContextDependencies,
            [NotNull] Func<IQueryBuffer> queryBufferFactory)
            : base(
                Check.NotNull(queryContextDependencies, nameof(queryContextDependencies)),
                Check.NotNull(queryBufferFactory, nameof(queryBufferFactory))
            )
        {
        }

        /// <inheritdoc />
        public override void BeginTrackingQuery()
        {
            Check.NotNull(QueryBuffer, nameof(QueryBuffer));
            base.BeginTrackingQuery();
        }
    }
}