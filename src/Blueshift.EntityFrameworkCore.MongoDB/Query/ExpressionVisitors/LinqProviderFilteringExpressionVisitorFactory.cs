using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query.ExpressionVisitors
{
    /// <inheritdoc />
    public class LinqProviderFilteringExpressionVisitorFactory
        : ILinqProviderFilteringExpressionVisitorFactory
    {
        [NotNull] private readonly IQueryableMethodProvider _queryableMethodProvider;

        /// <inheritdoc />
        public LinqProviderFilteringExpressionVisitorFactory(
            [NotNull] IQueryableMethodProvider queryableMethodProvider)
        {
            _queryableMethodProvider = Check.NotNull(queryableMethodProvider, nameof(queryableMethodProvider));
        }

        /// <inheritdoc />
        public LinqProviderFilteringExpressionVisitor Create()
            => new LinqProviderFilteringExpressionVisitor(
                _queryableMethodProvider);
    }
}