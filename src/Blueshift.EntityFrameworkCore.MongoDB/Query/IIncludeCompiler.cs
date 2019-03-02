using JetBrains.Annotations;
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
        /// <param name="trackingQuery"><c>true</c> if the current query results should be tracked; otherwise <c>false</c>.</param>
        /// <param name="asyncQuery"><c>true</c> if the current query is an asynchronous query.</param>
        void CompileIncludes(
            [NotNull] QueryModel queryModel,
            bool trackingQuery,
            bool asyncQuery);
    }
}
