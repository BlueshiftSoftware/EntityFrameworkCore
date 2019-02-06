using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query.Expressions
{
    /// <summary>
    /// Interface for a service that can be used to generate a document query expression.
    /// </summary>
    public interface IDocumentQueryExpressionFactory
    {
        /// <summary>
        /// Creates an <see cref="Expression"/> that represents a query for documents of a given entity type.
        /// </summary>
        /// <param name="entityType">The <see cref="IEntityType"/> that represents that documents to query.</param>
        /// <returns>An <see cref="Expression"/> that represents a query for documents of a given entity type.</returns>
        Expression CreateDocumentQueryExpression(IEntityType entityType);
    }
}
