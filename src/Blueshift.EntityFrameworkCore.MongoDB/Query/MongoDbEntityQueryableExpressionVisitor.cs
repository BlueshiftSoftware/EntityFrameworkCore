using System;
using System.Linq.Expressions;
using System.Reflection;
using Blueshift.EntityFrameworkCore.MongoDB.Storage;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query
{
    /// <summary>
    /// A visitor for processing MongoDB entity type roots.
    /// </summary>
    public class MongoDbEntityQueryableExpressionVisitor : EntityQueryableExpressionVisitor
    {
        private readonly IMongoDbConnection _mongoDbConnection;

        private static readonly MethodInfo _queryMethod = typeof(IMongoDbConnection).GetTypeInfo()
            .GetMethod(nameof(IMongoDbConnection.Query))
            .GetGenericMethodDefinition();

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDbEntityQueryableExpressionVisitor"/> class.
        /// </summary>
        /// <param name="entityQueryModelVisitor">The visitor for the query.</param>
        /// <param name="mongoDbConnection">The <see cref="MongoDbConnection"/> used to process the query.</param>
        public MongoDbEntityQueryableExpressionVisitor(
            [NotNull] EntityQueryModelVisitor entityQueryModelVisitor,
            [NotNull] IMongoDbConnection mongoDbConnection)
            : base(entityQueryModelVisitor)
        {
            _mongoDbConnection = Check.NotNull(mongoDbConnection, nameof(mongoDbConnection));
        }

        /// <summary>
        /// Visits entity type roots.
        /// </summary>
        /// <param name="elementType">The entity type of the root.</param>
        /// <returns>A new <see cref="Expression"/> to use in place of the original expression element.</returns>
        protected override Expression VisitEntityQueryable(Type elementType)
            => Expression.Call(
                Expression.Constant(_mongoDbConnection),
                _queryMethod.MakeGenericMethod(elementType));
    }
}