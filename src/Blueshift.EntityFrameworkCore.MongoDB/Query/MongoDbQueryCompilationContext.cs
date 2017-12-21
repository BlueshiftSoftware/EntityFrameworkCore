using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Remotion.Linq;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query
{
    /// <inheritdoc />
    public class MongoDbQueryCompilationContext : QueryCompilationContext
    {
        /// <inheritdoc />
        public MongoDbQueryCompilationContext(
            QueryCompilationContextDependencies dependencies,
            ILinqOperatorProvider linqOperatorProvider,
            bool trackQueryResults)
            : base(dependencies, linqOperatorProvider, trackQueryResults)
        {
        }

        /// <inheritdoc />
        public override void FindQuerySourcesRequiringMaterialization(
            EntityQueryModelVisitor queryModelVisitor,
            QueryModel queryModel)
        {
        }

        /// <inheritdoc />
        public override void DetermineQueryBufferRequirement(QueryModel queryModel)
        {
        }
    }
}