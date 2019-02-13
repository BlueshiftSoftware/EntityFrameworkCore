using System.Linq.Expressions;
using System.Reflection;
using Blueshift.EntityFrameworkCore.MongoDB.Storage;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query.Expressions
{
    /// <inheritdoc />
    public class MongoDbDocumentQueryExpressionFactory : IDocumentQueryExpressionFactory
    {
        private static readonly MethodInfo QueryMethodInfo
            = MethodHelper.GetGenericMethodDefinition<IMongoDbConnection, object>(
                mongoDbConnection => mongoDbConnection.Query<object>());

        private readonly IMongoDbConnection _mongoDbConnection;

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDbDocumentQueryExpressionFactory"/> class.
        /// </summary>
        /// <param name="mongoDbConnection">The <see cref="IMongoDbConnection"/> used to connect to the instance of MongoDb.</param>
        public MongoDbDocumentQueryExpressionFactory(
            [NotNull] IMongoDbConnection mongoDbConnection)
        {
            _mongoDbConnection = Check.NotNull(mongoDbConnection, nameof(mongoDbConnection));
        }

        /// <inheritdoc />
        public Expression CreateDocumentQueryExpression(IEntityType entityType)
            => Expression.Call(
                Expression.Constant(_mongoDbConnection),
                QueryMethodInfo.MakeGenericMethod(
                    entityType.GetMongoDbCollectionEntityType().ClrType));
    }
}