using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query.ExpressionVisitors
{
    /// <inheritdoc />
    public class MongoDbNavigationRewritingExpressionVisitorFactory : NavigationRewritingExpressionVisitorFactory
    {
        /// <inheritdoc />
        public override NavigationRewritingExpressionVisitor Create(EntityQueryModelVisitor queryModelVisitor)
            => new MongoDbNavigationRewritingExpressionVisitor(queryModelVisitor);
    }
}
