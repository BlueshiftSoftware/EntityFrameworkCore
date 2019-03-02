using System.Linq;
using System.Linq.Expressions;
using Blueshift.EntityFrameworkCore.MongoDB.Query.ExpressionVisitors;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query
{
    /// <inheritdoc />
    public class DocumentEntityQueryModelVisitorServiceFactory : IEntityQueryModelVisitorServiceFactory
    {
        [NotNull] private readonly EntityQueryModelVisitorDependencies _entityQueryModelVisitorDependencies;
        [NotNull] private readonly IQueryableMethodProvider _queryableMethodProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentEntityQueryModelVisitorServiceFactory"/> class.
        /// </summary>
        /// <param name="queryableMethodProvider">The <see cref="QueryableMethodProvider"/> used to reference <see cref="Queryable"/> methods.</param>
        /// <param name="entityQueryModelVisitorDependencies">The dependencies for the target <see cref="EntityQueryModelVisitor"/>.</param>
        public DocumentEntityQueryModelVisitorServiceFactory(
            [NotNull] IQueryableMethodProvider queryableMethodProvider,
            [NotNull] EntityQueryModelVisitorDependencies entityQueryModelVisitorDependencies)
        {
            _queryableMethodProvider = Check.NotNull(queryableMethodProvider, nameof(queryableMethodProvider));
            _entityQueryModelVisitorDependencies
                = Check.NotNull(
                    entityQueryModelVisitorDependencies,
                    nameof(entityQueryModelVisitorDependencies));
        }

        /// <inheritdoc />
        public IIncludeCompiler CreateIncludeCompiler(QueryCompilationContext queryCompilationContext)
            => new DocumentIncludeCompiler(
                Check.NotNull(queryCompilationContext, nameof(queryCompilationContext)),
                _entityQueryModelVisitorDependencies.QuerySourceTracingExpressionVisitorFactory);

        /// <inheritdoc />
        public RewritingExpressionVisitor CreateRewritingExpressionVisitor()
            => new RewritingExpressionVisitor(
                _queryableMethodProvider);

        /// <inheritdoc />
        public DenormalizationCompensatingExpressionVisitor CreateDenormalizationCompensatingExpressionVisitor()
            => new DenormalizationCompensatingExpressionVisitor();

        /// <inheritdoc />
        public NavigationRewritingExpressionVisitor CreateNavigationRewritingExpressionVisitor(
            EntityQueryModelVisitor entityQueryModelVisitor)
            => _entityQueryModelVisitorDependencies
                .NavigationRewritingExpressionVisitorFactory
                .Create(entityQueryModelVisitor);

        /// <inheritdoc />
        public ModelExpressionApplyingExpressionVisitor CreateModelExpressionApplyingExpressionVisitor(
            QueryCompilationContext queryCompilationContext,
            EntityQueryModelVisitor entityQueryModelVisitor)
            => new ModelExpressionApplyingExpressionVisitor(
                Check.NotNull(queryCompilationContext, nameof(queryCompilationContext)),
                _entityQueryModelVisitorDependencies.QueryModelGenerator,
                Check.NotNull(entityQueryModelVisitor, nameof(entityQueryModelVisitor)));

        /// <inheritdoc />
        public ExpressionVisitor CreateProjectionExpressionVisitor(
            EntityQueryModelVisitor entityQueryModelVisitor,
            IQuerySource querySource)
            => _entityQueryModelVisitorDependencies
                .ProjectionExpressionVisitorFactory
                .Create(
                    entityQueryModelVisitor,
                    querySource);
    }
}