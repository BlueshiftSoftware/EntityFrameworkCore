using System.Linq.Expressions;
using Blueshift.EntityFrameworkCore.MongoDB.Query.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query.ExpressionVisitors
{
    /// <inheritdoc />
    public class MongoDbEntityQueryableExpressionVisitorFactory : IEntityQueryableExpressionVisitorFactory
    {
        private readonly IModel _model;
        private readonly IDocumentQueryExpressionFactory _documentQueryExpressionFactory;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public MongoDbEntityQueryableExpressionVisitorFactory(
            [NotNull] IModel model,
            [NotNull] IDocumentQueryExpressionFactory documentQueryExpressionFactory)
        {
            _model = Check.NotNull(model, nameof(model));
            _documentQueryExpressionFactory = Check.NotNull(documentQueryExpressionFactory, nameof(documentQueryExpressionFactory));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ExpressionVisitor Create(
            EntityQueryModelVisitor entityQueryModelVisitor,
            IQuerySource querySource)
            => new MongoDbEntityQueryableExpressionVisitor(
                Check.Is<MongoDbEntityQueryModelVisitor>(entityQueryModelVisitor, nameof(entityQueryModelVisitor)),
                _model,
                querySource,
                _documentQueryExpressionFactory);
    }
}