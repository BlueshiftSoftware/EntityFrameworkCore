using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query
{
    /// <inheritdoc />
    public class LinqQueryCompilationContextFactory : QueryCompilationContextFactory
    {
        /// <inheritdoc />
        public LinqQueryCompilationContextFactory(
            [NotNull] QueryCompilationContextDependencies dependencies) : base(dependencies)
        {
        }

        /// <inheritdoc />
        public override QueryCompilationContext Create(bool async)
            => new QueryCompilationContext(
                Dependencies,
                new QueryableLinqOperatorProvider(),
                TrackQueryResults);
    }
}
