using Blueshift.EntityFrameworkCore.MongoDB.Storage;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query
{
    /// <summary>
    ///     A factory for <see cref="MongoDbQueryContext"/> instances.
    /// </summary>
    public class MongoDbQueryContextFactory : QueryContextFactory
    {
        private readonly IMongoDbConnection _mongoDbConnection;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended
        ///     to be used directly from your code. This API may change or be removed in future
        ///     releases.
        /// </summary>
        public MongoDbQueryContextFactory(
            [NotNull] QueryContextDependencies queryContextDependencies,
            [NotNull] IMongoDbConnection mongoDbConnection)
            : base(Check.NotNull(queryContextDependencies, nameof(queryContextDependencies)))
        {
            _mongoDbConnection = Check.NotNull(mongoDbConnection, nameof(mongoDbConnection));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended
        ///     to be used directly from your code. This API may change or be removed in future
        ///     releases.
        /// </summary>
        public override QueryContext Create()
            => new MongoDbQueryContext(Dependencies, CreateQueryBuffer, _mongoDbConnection);
    }
}