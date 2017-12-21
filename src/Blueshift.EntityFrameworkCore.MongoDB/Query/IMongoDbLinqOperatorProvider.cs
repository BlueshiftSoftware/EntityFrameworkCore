using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query
{
    /// <inheritdoc />
    public interface IMongoDbLinqOperatorProvider : ILinqOperatorProvider
    {
        /// <inheritdoc cref="ILinqOperatorProvider.OrderBy"/>
        MethodInfo OrderByDescending { get; }

        /// <inheritdoc cref="ILinqOperatorProvider.ThenBy"/>
        MethodInfo ThenByDescending { get; }

        /// <inheritdoc cref="ILinqOperatorProvider.ToOrdered"/>
        MethodInfo ToList { get; }
    }
}