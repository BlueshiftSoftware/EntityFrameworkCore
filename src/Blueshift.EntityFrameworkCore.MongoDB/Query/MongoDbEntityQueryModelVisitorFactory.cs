using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.Query
{

    /// <summary>
    ///     Creates instances of <see cref="MongoDbEntityQueryModelVisitor"/>.
    ///     This type is typically used by database providers (and other extensions). It
    ///     is generally not used in application code.
    /// </summary>
    public class MongoDbEntityQueryModelVisitorFactory : EntityQueryModelVisitorFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDbEntityQueryModelVisitorFactory"/> class.
        /// </summary>
        /// <param name="entityQueryModelVisitorDependencies">Parameter object that contains dependencies for this service.</param>
        public MongoDbEntityQueryModelVisitorFactory([NotNull] EntityQueryModelVisitorDependencies entityQueryModelVisitorDependencies)
            : base(Check.NotNull(entityQueryModelVisitorDependencies, nameof(entityQueryModelVisitorDependencies)))
        {
        }

        /// <summary>
        ///     Creates a new <see cref="MongoDbEntityQueryModelVisitor"/>.
        /// </summary>
        /// <param name="queryCompilationContext">Compilation context for the query.</param>
        /// <param name="parentEntityQueryModelVisitor">The visitor for the outer query.</param>
        /// <returns>The newly created <see cref="MongoDbEntityQueryModelVisitor"/>.</returns>
        public override EntityQueryModelVisitor Create(QueryCompilationContext queryCompilationContext,
                EntityQueryModelVisitor parentEntityQueryModelVisitor)
            => new MongoDbEntityQueryModelVisitor(Dependencies, queryCompilationContext);
    }
}