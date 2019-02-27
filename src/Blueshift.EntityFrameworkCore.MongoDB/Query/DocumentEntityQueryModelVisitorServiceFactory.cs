using System.Linq;
using Blueshift.EntityFrameworkCore.MongoDB.Query.ExpressionVisitors;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query
{
    /// <inheritdoc />
    public class DocumentEntityQueryModelVisitorServiceFactory : IEntityQueryModelVisitorServiceFactory
    {
        [NotNull] private readonly IQueryableMethodProvider _queryableMethodProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentEntityQueryModelVisitorServiceFactory"/> class.
        /// </summary>
        /// <param name="queryableMethodProvider">The <see cref="QueryableMethodProvider"/> used to reference <see cref="Queryable"/> methods.</param>
        public DocumentEntityQueryModelVisitorServiceFactory(
            [NotNull] IQueryableMethodProvider queryableMethodProvider)
        {
            _queryableMethodProvider = Check.NotNull(queryableMethodProvider, nameof(queryableMethodProvider));
        }

        /// <inheritdoc />
        public IIncludeCompiler CreateIncludeCompiler()
            => new DocumentIncludeCompiler();

        /// <inheritdoc />
        public RewritingExpressionVisitor CreateRewritingExpressionVisitor()
            => new RewritingExpressionVisitor(
                _queryableMethodProvider);

        /// <inheritdoc />
        public DenormalizationCompensatingExpressionVisitor CreateDenormalizationCompensatingExpressionVisitor()
            => new DenormalizationCompensatingExpressionVisitor();
    }
}