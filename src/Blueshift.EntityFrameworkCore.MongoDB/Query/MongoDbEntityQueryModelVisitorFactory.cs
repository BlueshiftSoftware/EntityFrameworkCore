using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.Query
{
    public class MongoDbEntityQueryModelVisitorFactory : EntityQueryModelVisitorFactory
    {
        public MongoDbEntityQueryModelVisitorFactory([NotNull] EntityQueryModelVisitorDependencies entityQueryModelVisitorDependencies)
            : base(Check.NotNull(entityQueryModelVisitorDependencies, nameof(entityQueryModelVisitorDependencies)))
        {
        }

        public override EntityQueryModelVisitor Create(QueryCompilationContext queryCompilationContext,
                EntityQueryModelVisitor parentEntityQueryModelVisitor)
            => new MongoDbEntityQueryModelVisitor(Dependencies, queryCompilationContext);
    }
}