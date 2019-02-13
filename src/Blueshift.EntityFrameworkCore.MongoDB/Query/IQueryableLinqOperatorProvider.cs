using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query
{
    /// <inheritdoc />
    public interface IQueryableLinqOperatorProvider : ILinqOperatorProvider
    {
        /// <inheritdoc cref="ILinqOperatorProvider.OrderBy"/>
        MethodInfo OrderByDescending { get; }

        /// <inheritdoc cref="ILinqOperatorProvider.ThenBy"/>
        MethodInfo ThenByDescending { get; }
    }
}