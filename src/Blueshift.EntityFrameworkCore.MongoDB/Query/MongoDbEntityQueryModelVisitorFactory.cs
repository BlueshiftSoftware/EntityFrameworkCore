using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class MongoDbEntityQueryModelVisitorFactory : EntityQueryModelVisitorFactory
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public MongoDbEntityQueryModelVisitorFactory(
            [NotNull] EntityQueryModelVisitorDependencies entityQueryModelVisitorDependencies)
            : base(Check.NotNull(entityQueryModelVisitorDependencies, nameof(entityQueryModelVisitorDependencies)))
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override EntityQueryModelVisitor Create(QueryCompilationContext queryCompilationContext,
                EntityQueryModelVisitor parentEntityQueryModelVisitor)
            => new MongoDbEntityQueryModelVisitor(Dependencies, queryCompilationContext);
    }
}