using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Blueshift.EntityFrameworkCore.MongoDB.Query.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
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
            // ReSharper disable once SuggestBaseTypeForParameter
            [NotNull] MongoDbEntityQueryModelVisitor entityQueryModelVisitor,
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

            entityType = entityType.GetMongoDbCollectionEntityType();

            var documentQueryExpression = _documentQueryExpressionFactory
                .CreateDocumentQueryExpression(entityType);

            if (entityType.ClrType != elementType)
            {
                MethodInfo ofTypeMethodInfo = MethodHelper
                    .GetGenericMethodDefinition<object>(() => Enumerable.OfType<object>(null))
                    .MakeGenericMethod(elementType);

                documentQueryExpression = Expression.Call(
                    null,
                    ofTypeMethodInfo,
                    documentQueryExpression);
            }

            return documentQueryExpression;
        }
    }
}