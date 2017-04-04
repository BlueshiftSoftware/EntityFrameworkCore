using System;
using System.Linq.Expressions;
using Blueshift.EntityFrameworkCore.MongoDB.Storage;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class MongoDbEntityQueryableExpressionVisitor : EntityQueryableExpressionVisitor
    {
        private readonly IModel _model;
        private readonly IMongoDbConnection _mongoDbConnection;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public MongoDbEntityQueryableExpressionVisitor(
            [NotNull] EntityQueryModelVisitor entityQueryModelVisitor,
            [NotNull] IModel model,
            [NotNull] IMongoDbConnection mongoDbConnection)
            : base(entityQueryModelVisitor)
        {
            _model = Check.NotNull(model, nameof(model));
            _mongoDbConnection = Check.NotNull(mongoDbConnection, nameof(mongoDbConnection));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitEntityQueryable(Type elementType)
        {
            var entityType = _model.FindEntityType(Check.NotNull(elementType, nameof(elementType)));
            var annotations = entityType.MongoDb();
            while (annotations.IsDerivedType && entityType.BaseType != null)
            {
                entityType = entityType.BaseType;
                annotations = entityType.MongoDb();
            }

            return (entityType.ClrType == elementType)
                ? Expression.Call(
                    MongoDbEntityQueryModelVisitor.EntityQueryMethodInfo.MakeGenericMethod(elementType),
                    EntityQueryModelVisitor.QueryContextParameter)
                : Expression.Call(
                    MongoDbEntityQueryModelVisitor.SubEntityQueryMethodInfo.MakeGenericMethod(entityType.ClrType, elementType),
                    EntityQueryModelVisitor.QueryContextParameter);
        }
    }
}