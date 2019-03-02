using System.Linq.Expressions;
using Blueshift.EntityFrameworkCore.MongoDB.Query.ExpressionVisitors;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Remotion.Linq.Clauses;

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
        /// <param name="queryCompilationContext">The context used to compile the current query.</param>
        /// <returns>A new instance of <see cref="IIncludeCompiler"/> for compiling queryable <c>Include()</c> operators.</returns>
        IIncludeCompiler CreateIncludeCompiler([NotNull] QueryCompilationContext queryCompilationContext);


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

        /// <summary>
        ///     Creates a <see cref="NavigationRewritingExpressionVisitor"/> that can be used to rewrite navigation expressions
        ///     into joins.
        /// </summary>
        /// <returns>A new instance of <see cref="NavigationRewritingExpressionVisitor"/>.</returns>
        NavigationRewritingExpressionVisitor CreateNavigationRewritingExpressionVisitor(
            [NotNull] EntityQueryModelVisitor entityQueryModelVisitor);

        /// <summary>
        ///     Creates a <see cref="ModelExpressionApplyingExpressionVisitor"/> for the current query.
        /// </summary>
        /// <param name="queryCompilationContext">The <see cref="QueryCompilationContext"/> used to compile the current query.</param>
        /// <param name="entityQueryModelVisitor">The <see cref="EntityQueryModelVisitor"/> used to process the current query model.</param>
        /// <returns>A new <see cref="ModelExpressionApplyingExpressionVisitor"/>.</returns>
        ModelExpressionApplyingExpressionVisitor CreateModelExpressionApplyingExpressionVisitor(
            QueryCompilationContext queryCompilationContext,
            EntityQueryModelVisitor entityQueryModelVisitor);

        /// <summary>
        ///     Creates a <see cref="ExpressionVisitor"/> that processes projection expressions.
        /// </summary>
        /// <param name="entityQueryModelVisitor">The <see cref="EntityQueryModelVisitor"/> processing the current query model.</param>
        /// <param name="querySource">The current <see cref="IQuerySource"/> being processed.</param>
        /// <returns>A new <see cref="ExpressionVisitor"/> used to process a projection expression tree.</returns>
        ExpressionVisitor CreateProjectionExpressionVisitor(
            [NotNull] EntityQueryModelVisitor entityQueryModelVisitor,
            [NotNull] IQuerySource querySource);
    }
}
