using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;

namespace Blueshift.EntityFrameworkCore.Query
{
    /// <summary>
    ///     A factory for <see cref="MongoDbQueryContext"/> instances.
    /// </summary>
    public class MongoDbQueryContextFactory : QueryContextFactory
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended
        ///     to be used directly from your code. This API may change or be removed in future
        ///     releases.
        /// </summary>
        /// <param name="queryContextDependencies">Parameter object containing dependencies for this service</param>
        public MongoDbQueryContextFactory(
            [NotNull] QueryContextDependencies queryContextDependencies)
            : base(queryContextDependencies)
        {
        }

        /// <summary>
        ///     Creates a new <see cref="MongoDbQueryContext"/>.
        /// </summary>
        /// <returns>The newly created <see cref="MongoDbQueryContext"/>.</returns>
        public override QueryContext Create()
            => new MongoDbQueryContext(Dependencies, CreateQueryBuffer);
    }
}