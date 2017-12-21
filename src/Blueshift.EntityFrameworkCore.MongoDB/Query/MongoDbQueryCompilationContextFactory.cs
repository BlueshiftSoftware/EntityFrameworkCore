using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query
{
    /// <inheritdoc />
    public class MongoDbQueryCompilationContextFactory : QueryCompilationContextFactory
    {
        /// <inheritdoc />
        public MongoDbQueryCompilationContextFactory(
            [NotNull] QueryCompilationContextDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <inheritdoc />
        public override QueryCompilationContext Create(bool async)
            => new MongoDbQueryCompilationContext(
                Dependencies,
                async
                    ? (ILinqOperatorProvider)new AsyncLinqOperatorProvider()
                    : new MongoDbLinqOperatorProvider(),
                TrackQueryResults);
    }
}