using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.Query
{
    /// <summary>
    ///     The core visitor that processes a query to be executed.
    ///     This type is typically used by database providers (and other extensions). It
    ///     is generally not used in application code.
    /// </summary>
    public class MongoDbEntityQueryModelVisitor : EntityQueryModelVisitor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDbEntityQueryModelVisitor"/> class.
        /// </summary>
        /// <param name="entityQueryModelVisitorDependencies">Parameter object containing dependencies for this service.</param>
        /// <param name="queryCompilationContext">The <see cref="QueryCompilationContext"/> to be used when processing the query.</param>
        public MongoDbEntityQueryModelVisitor(
            [NotNull] EntityQueryModelVisitorDependencies entityQueryModelVisitorDependencies,
            [NotNull] QueryCompilationContext queryCompilationContext)
            : base(
                Check.NotNull(entityQueryModelVisitorDependencies, nameof(entityQueryModelVisitorDependencies)),
                Check.NotNull(queryCompilationContext, nameof(queryCompilationContext))
            )
        {
        }
    }
}