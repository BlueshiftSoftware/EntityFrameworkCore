namespace Blueshift.EntityFrameworkCore.MongoDB.Query.ExpressionVisitors
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public interface INullConditionalExpressionCompensatingExpressionVisitorFactory
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        NullConditionalExpressionCompensatingExpressionVisitor Create();
    }
}
