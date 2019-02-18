using System.Linq;
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
        [NotNull] private readonly IQueryableMethodProvider _queryableMethodProvider;
        [NotNull] private readonly IModel _model;

        private static readonly MethodInfo QueryMethodInfo
            = MethodHelper.GetGenericMethodDefinition<IMongoDbConnection, object>(
                mongoDbConnection => mongoDbConnection.Query<object>());

        private readonly IMongoDbConnection _mongoDbConnection;

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDbDocumentQueryExpressionFactory"/> class.
        /// </summary>
        /// <param name="model">The <see cref="IModel"/> for the current data set.</param>
        /// <param name="mongoDbConnection">The <see cref="IMongoDbConnection"/> used to connect to the instance of MongoDb.</param>
        /// <param name="queryableMethodProvider">The <see cref="IQueryableMethodProvider"/> used to reference <see cref="IQueryable"/> methods.</param>
        public MongoDbDocumentQueryExpressionFactory(
            [NotNull] IModel model,
            [NotNull] IMongoDbConnection mongoDbConnection,
            [NotNull] IQueryableMethodProvider queryableMethodProvider)
        {
            _model = Check.NotNull(model, nameof(model));
            _mongoDbConnection = Check.NotNull(mongoDbConnection, nameof(mongoDbConnection));
            _queryableMethodProvider = Check.NotNull(queryableMethodProvider, nameof(queryableMethodProvider));
        }

        /// <inheritdoc />
        public Expression CreateDocumentQueryExpression(IEntityType entityType)
        {
            IEntityType collectionEntityType = entityType.GetMongoDbCollectionEntityType();

            Expression expression = Expression.Call(
                Expression.Constant(_mongoDbConnection),
                QueryMethodInfo.MakeGenericMethod(collectionEntityType.ClrType));

            if (collectionEntityType != entityType)
            {
                expression = Expression.Call(
                    _queryableMethodProvider.OfType.MakeGenericMethod(entityType.ClrType),
                    expression);
            }

            return expression;
        }
    }
}