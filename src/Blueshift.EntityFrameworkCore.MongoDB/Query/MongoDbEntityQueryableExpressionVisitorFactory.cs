using System.Linq.Expressions;
using Blueshift.EntityFrameworkCore.MongoDB.Storage;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query
{
    /// <summary>
    ///     A factory for creating instances of <see cref="MongoDbEntityQueryableExpressionVisitor" />.
    /// </summary>
    public class MongoDbEntityQueryableExpressionVisitorFactory : IEntityQueryableExpressionVisitorFactory
    {
        private readonly IMongoDbConnection _mongoDbConnection;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MongoDbEntityQueryableExpressionVisitorFactory"/> class.
        /// </summary>
        /// <param name="mongoDbConnection">The <see cref="MongoDbConnection"/> used to process the query.</param>
        public MongoDbEntityQueryableExpressionVisitorFactory([NotNull] IMongoDbConnection mongoDbConnection)
        {
            _mongoDbConnection = Check.NotNull(mongoDbConnection, nameof(mongoDbConnection));
        }

        /// <summary>
        ///     Creates a new <see cref="MongoDbEntityQueryableExpressionVisitor"/>.
        /// </summary>
        /// <param name="entityQueryModelVisitor">The query model visitor.</param>
        /// <param name="querySource">The query source.</param>
        /// <returns>A new instance of <see cref="MongoDbEntityQueryableExpressionVisitor"/>.</returns>
        public virtual ExpressionVisitor Create(
            [NotNull] EntityQueryModelVisitor entityQueryModelVisitor,
            [CanBeNull] IQuerySource querySource)
            =>  new MongoDbEntityQueryableExpressionVisitor(
                    Check.NotNull(entityQueryModelVisitor, nameof(entityQueryModelVisitor)),
                    _mongoDbConnection);
    }
}