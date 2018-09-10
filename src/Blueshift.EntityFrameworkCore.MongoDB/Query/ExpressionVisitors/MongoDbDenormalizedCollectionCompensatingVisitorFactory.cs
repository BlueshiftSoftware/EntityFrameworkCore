namespace Blueshift.EntityFrameworkCore.MongoDB.Query.ExpressionVisitors
{
    /// <inheritdoc />
    public class MongoDbDenormalizedCollectionCompensatingVisitorFactory : IMongoDbDenormalizedCollectionCompensatingVisitorFactory
    {
        /// <inheritdoc />
        public MongoDbDenormalizedCollectionCompensatingVisitor Create()
            => new MongoDbDenormalizedCollectionCompensatingVisitor();
    }
}