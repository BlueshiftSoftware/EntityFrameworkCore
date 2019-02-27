using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.ResultOperators.Internal;
using Remotion.Linq;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query
{
    /// <summary>
    ///     Interface for a service that compile <see cref="EntityFrameworkQueryableExtensions.Include{TEntity, TProperty}"/> operators
    ///     into expression trees that can be used by the underlying provider.
    /// </summary>
    public interface IIncludeCompiler
    {
        /// <summary>
        /// Compiles <see cref="IncludeResultOperator"/> instances found on the target <paramref name="queryModel"/>.
        /// </summary>
        /// <param name="queryModel">The <see cref="QueryModel"/> containing the <c>.Include()</c> operators to compile.</param>
        void CompileInclude(QueryModel queryModel);
    }
}
