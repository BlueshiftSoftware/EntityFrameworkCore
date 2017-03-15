using System;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Utilities;
using Blueshift.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Query;

namespace Blueshift.EntityFrameworkCore.Query
{
    public class MongoDbEntityQueryableExpressionVisitor : EntityQueryableExpressionVisitor
    {
        private readonly IMongoDbConnection _mongoDbConnection;

        private static readonly MethodInfo _queryMethod = typeof(IMongoDbConnection).GetTypeInfo()
            .GetMethod(nameof(IMongoDbConnection.Query))
            .GetGenericMethodDefinition();

        public MongoDbEntityQueryableExpressionVisitor([NotNull] EntityQueryModelVisitor entityQueryModelVisitor,
            [NotNull] IMongoDbConnection mongoDbConnection)
            : base(entityQueryModelVisitor)
        {
            _mongoDbConnection = Check.NotNull(mongoDbConnection, nameof(mongoDbConnection));
        }

        protected override Expression VisitEntityQueryable(Type elementType)
            => Expression.Call(
                Expression.Constant(_mongoDbConnection),
                _queryMethod.MakeGenericMethod(elementType));
    }
}