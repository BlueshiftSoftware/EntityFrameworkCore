using System;
using Blueshift.EntityFrameworkCore.MongoDB.Storage;
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
            [NotNull] Func<IQueryBuffer> queryBufferFactory,
            [NotNull] IMongoDbConnection mongoDbConnection)
            : base(
                Check.NotNull(queryContextDependencies, nameof(queryContextDependencies)),
                Check.NotNull(queryBufferFactory, nameof(queryBufferFactory))
            )
        {
            MongoDbConnection = Check.NotNull(mongoDbConnection, nameof(mongoDbConnection));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IMongoDbConnection MongoDbConnection { get; }
    }
}