using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query.ExpressionVisitors
{
    /// <inheritdoc />
    public class LinqAdapterFilteringExpressionVisitorFactory
        : ILinqAdapterFilteringExpressionVisitorFactory
    {
        [NotNull] private readonly IQueryableMethodProvider _queryableMethodProvider;

        /// <inheritdoc />
        public LinqAdapterFilteringExpressionVisitorFactory(
            [NotNull] IQueryableMethodProvider queryableMethodProvider)
        {
            _queryableMethodProvider = Check.NotNull(queryableMethodProvider, nameof(queryableMethodProvider));
        }

        /// <inheritdoc />
        public ExpressionVisitor Create()
            => new LinqAdapterFilteringExpressionVisitor(
                _queryableMethodProvider);
    }
}