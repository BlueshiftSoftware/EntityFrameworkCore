using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Remotion.Linq.Parsing;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query.ExpressionVisitors
{
    /// <inheritdoc />
    public class NullConditionalExpressionCompensatingExpressionVisitor : RelinqExpressionVisitor
    {
        /// <inheritdoc />
        protected override Expression VisitExtension(Expression node)
        {
            if (node is NullConditionalExpression nullConditionalExpression)
            {
                node = nullConditionalExpression.AccessOperation;
            }

            return base.Visit(node);
        }
    }
}
