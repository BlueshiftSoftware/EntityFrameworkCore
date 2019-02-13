using System;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query.Expressions
{
    /// <inheritdoc />
    /// <summary>
    /// Represents a query for a set of documents from a document database.
    /// </summary>
    public class DocumentQueryExpression : Expression
    {
        private readonly IDocumentQueryExpressionFactory _documentQueryExpressionFactory;
        private readonly IEntityType _entityType;

        /// <inheritdoc />
        /// <summary>
        ///   Creates a new instance of the <see cref="DocumentQueryExpression"/> class.
        /// </summary>
        /// <param name="documentQueryExpressionFactory">The <see cref="IDocumentQueryExpressionFactory"/> to use to create
        /// the root document query expression.</param>
        /// <param name="entityType">The <see cref="IEntityType"/> representing the type of entities to query.</param>
        public DocumentQueryExpression(
            [NotNull] IDocumentQueryExpressionFactory documentQueryExpressionFactory,
            [NotNull] IEntityType entityType)
        {
            _documentQueryExpressionFactory = Check.NotNull(documentQueryExpressionFactory, nameof(documentQueryExpressionFactory));
            _entityType = Check.NotNull(entityType, nameof(entityType));
        }

        /// <inheritdoc />
        public override bool CanReduce => true;

        /// <inheritdoc />
        public override Expression Reduce()
            => _documentQueryExpressionFactory
                .CreateDocumentQueryExpression(_entityType);

        /// <inheritdoc />
        public override Type Type
            => typeof(IQueryable<>).MakeGenericType(_entityType.ClrType);

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor) => this;

        /// <inheritdoc />
        public override ExpressionType NodeType => ExpressionType.Extension;
    }
}
