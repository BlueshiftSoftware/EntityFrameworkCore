using System;
using System.Linq.Expressions;
using Blueshift.EntityFrameworkCore.MongoDB.Query.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query.ExpressionVisitors
{
    /// <inheritdoc />
    public class MongoDbEntityQueryableExpressionVisitor : EntityQueryableExpressionVisitor
    {
        private readonly IModel _model;
        private readonly IQuerySource _querySource;
        private readonly IDocumentQueryExpressionFactory _documentQueryExpressionFactory;

        /// <inheritdoc />
        public MongoDbEntityQueryableExpressionVisitor(
            [NotNull] EntityQueryModelVisitor entityQueryModelVisitor,
            [NotNull] IModel model,
            [CanBeNull] IQuerySource querySource,
            [NotNull] IDocumentQueryExpressionFactory documentQueryExpressionFactory)
            : base(entityQueryModelVisitor)
        {
            _model = Check.NotNull(model, nameof(model));
            _querySource = querySource;
            _documentQueryExpressionFactory = Check.NotNull(documentQueryExpressionFactory, nameof(documentQueryExpressionFactory));
        }

        private new MongoDbEntityQueryModelVisitor QueryModelVisitor => (MongoDbEntityQueryModelVisitor)base.QueryModelVisitor;

        /// <inheritdoc />
        protected override Expression VisitEntityQueryable(Type elementType)
        {
            Check.NotNull(elementType, nameof(elementType));

            IEntityType entityType = QueryModelVisitor.QueryCompilationContext.FindEntityType(_querySource)
                                     ?? _model.FindEntityType(elementType);

            return _documentQueryExpressionFactory
                .CreateDocumentQueryExpression(entityType);
        }
    }
}