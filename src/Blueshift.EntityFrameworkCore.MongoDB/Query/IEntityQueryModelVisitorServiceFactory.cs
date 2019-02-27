using Blueshift.EntityFrameworkCore.MongoDB.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Query;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query
{
    /// <summary>
    ///     Interface for a service that can be used to create services used by an <see cref="EntityQueryModelVisitor"/>.
    /// </summary>
    public interface IEntityQueryModelVisitorServiceFactory
    {
        /// <summary>
        ///     Creates a <see cref="IIncludeCompiler"/> used by the underlying <see cref="EntityQueryModelVisitor"/>.
        /// </summary>
        /// <returns>A new instance of <see cref="IIncludeCompiler"/> for compiling queryable <c>Include()</c> operators.</returns>
        IIncludeCompiler CreateIncludeCompiler();


        /// <summary>
        ///     Creates a <see cref="RewritingExpressionVisitor"/> that can be used to rewrite expression trees that cannot be
        ///     understood by the underlying query provider.
        /// </summary>
        /// <returns>A new instance of <see cref="RewritingExpressionVisitor"/>.</returns>
        RewritingExpressionVisitor CreateRewritingExpressionVisitor();

        /// <summary>
        ///     Creates a <see cref="DenormalizationCompensatingExpressionVisitor"/> that can be used to compensate for denormalized
        ///     properties in a join expression.
        /// </summary>
        /// <returns>A new instance of <see cref="DenormalizationCompensatingExpressionVisitor"/>.</returns>
        DenormalizationCompensatingExpressionVisitor CreateDenormalizationCompensatingExpressionVisitor();
    }
}
