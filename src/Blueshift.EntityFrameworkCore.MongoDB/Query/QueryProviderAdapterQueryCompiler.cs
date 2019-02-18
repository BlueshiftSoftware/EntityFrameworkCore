using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query
{
    /// <inheritdoc />
    public class QueryProviderAdapterQueryCompiler : QueryCompiler
    {
        /// <inheritdoc />
        public QueryProviderAdapterQueryCompiler(
            [NotNull] IQueryContextFactory queryContextFactory,
            [NotNull] ICompiledQueryCache compiledQueryCache,
            [NotNull] ICompiledQueryCacheKeyGenerator compiledQueryCacheKeyGenerator,
            [NotNull] IDatabase database,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Query> logger,
            [NotNull] ICurrentDbContext currentContext,
            [NotNull] IQueryModelGenerator queryModelGenerator)
            : base(
                queryContextFactory,
                compiledQueryCache,
                compiledQueryCacheKeyGenerator,
                database,
                logger,
                currentContext,
                queryModelGenerator)
        {
        }

        /// <inheritdoc />
        public override IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression query)
            => Execute<IEnumerable<TResult>>(query)
                .ToAsyncEnumerable();

        /// <inheritdoc />
        public override Task<TResult> ExecuteAsync<TResult>(
            Expression query,
            CancellationToken cancellationToken)
            => Task.Run(
                () => Execute<TResult>(query),
                cancellationToken);
    }
}
