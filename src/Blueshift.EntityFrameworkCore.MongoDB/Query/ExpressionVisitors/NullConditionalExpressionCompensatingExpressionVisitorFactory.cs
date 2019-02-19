namespace Blueshift.EntityFrameworkCore.MongoDB.Query.ExpressionVisitors
{
    /// <inheritdoc />
    public class NullConditionalExpressionCompensatingExpressionVisitorFactory
        : INullConditionalExpressionCompensatingExpressionVisitorFactory
    {
        /// <inheritdoc />
        public NullConditionalExpressionCompensatingExpressionVisitor Create()
            => new NullConditionalExpressionCompensatingExpressionVisitor();
    }
}