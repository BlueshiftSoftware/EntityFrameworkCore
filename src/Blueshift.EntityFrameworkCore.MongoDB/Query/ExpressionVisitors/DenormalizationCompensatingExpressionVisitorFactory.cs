using System.Linq.Expressions;
using Remotion.Linq.Parsing;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query.ExpressionVisitors
{
    /// <inheritdoc />
    public class DenormalizationCompensatingExpressionVisitorFactory : IDenormalizationCompensatingExpressionVisitorFactory
    {
        /// <inheritdoc />
        public ExpressionVisitor Create()
            => new DenormalizationCompensatingExpressionVisitor();
    }
}