using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Blueshift.EntityFrameworkCore.MongoDB.Metadata;
using Blueshift.EntityFrameworkCore.MongoDB.Metadata.Builders;
using Blueshift.EntityFrameworkCore.MongoDB.Storage;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Driver;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query.Expressions
{
    /// <inheritdoc />
    public class MongoDbDocumentQueryExpressionFactory : IDocumentQueryExpressionFactory
    {
        private static readonly MethodInfo GetCollectionMethodInfo
            = MethodHelper.GetGenericMethodDefinition<IMongoDatabase, object>(
                mongoDatabase => mongoDatabase.GetCollection<object>("", null));

        private static readonly MethodInfo AsQueryableMethodInfo
            = MethodHelper.GetGenericMethodDefinition<IMongoCollection<object>, object>(
                mongoCollection => mongoCollection.AsQueryable(null));

        private static readonly MethodInfo OfTypeMethodInfo
            = MethodHelper.GetGenericMethodDefinition<IQueryable<object>, object>(
                queryable => queryable.OfType<object>());

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
        {
            MongoDbEntityTypeAnnotations annotations = Check.NotNull(entityType, nameof(entityType)).MongoDb();

            IEntityType queryEntityType = entityType;

            if (!entityType.IsDocumentRootEntityType())
            {
                entityType = entityType.GetMongoDbCollectionEntityType();
            }

            Expression queryExpression = Expression.Call(
                Expression.Constant(_mongoDbConnection.GetDatabase()),
                GetCollectionMethodInfo.MakeGenericMethod(entityType.ClrType),
                new Expression[]
                {
                    Expression.Constant(annotations.CollectionName),
                    Expression.Constant(
                        annotations.CollectionSettings,
                        typeof(MongoCollectionSettings))
                });

            queryExpression = Expression.Call(
                null,
                AsQueryableMethodInfo.MakeGenericMethod(entityType.ClrType),
                new []
                {
                    queryExpression,
                    Expression.Constant(
                        null,
                        typeof(AggregateOptions))
                });

            if (queryEntityType != entityType)
            {
                queryExpression = Expression.Call(
                    queryExpression,
                    OfTypeMethodInfo.MakeGenericMethod(queryEntityType.ClrType));
            }

            return queryExpression;
        }
    }
}