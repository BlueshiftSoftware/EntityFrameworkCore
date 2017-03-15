using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;
using Blueshift.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Query;

namespace Blueshift.EntityFrameworkCore.Query
{
    public class MongoDbEntityQueryableExpressionVisitorFactory : IEntityQueryableExpressionVisitorFactory
    {
        private readonly IMongoDbConnection _mongoDbConnection;

        public MongoDbEntityQueryableExpressionVisitorFactory([NotNull] IMongoDbConnection mongoDbConnection)
        {
            _mongoDbConnection = Check.NotNull(mongoDbConnection, nameof(mongoDbConnection));
        }

        public virtual ExpressionVisitor Create([NotNull] EntityQueryModelVisitor entityQueryModelVisitor,
            [CanBeNull] IQuerySource querySource)
            =>  new MongoDbEntityQueryableExpressionVisitor(
                    Check.NotNull(entityQueryModelVisitor, nameof(entityQueryModelVisitor)),
                    _mongoDbConnection);
    }
}