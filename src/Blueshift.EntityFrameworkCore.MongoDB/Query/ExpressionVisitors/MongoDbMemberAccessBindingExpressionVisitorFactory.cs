using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Remotion.Linq.Clauses;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query.ExpressionVisitors
{
    /// <inheritdoc />
    public class MongoDbMemberAccessBindingExpressionVisitorFactory : MemberAccessBindingExpressionVisitorFactory
    {
        /// <inheritdoc />
        public override ExpressionVisitor Create(
            QuerySourceMapping querySourceMapping,
            EntityQueryModelVisitor queryModelVisitor,
            bool inProjection)
            => new MongoDbMemberAccessBindingExpressionVisitor(
                querySourceMapping,
                queryModelVisitor,
                inProjection);
    }
}
