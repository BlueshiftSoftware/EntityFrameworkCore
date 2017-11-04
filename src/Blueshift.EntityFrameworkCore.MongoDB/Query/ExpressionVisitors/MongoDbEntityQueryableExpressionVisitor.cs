using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Blueshift.EntityFrameworkCore.MongoDB.Metadata;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query.ExpressionVisitors
{
    /// <inheritdoc />
    public class MongoDbEntityQueryableExpressionVisitor : EntityQueryableExpressionVisitor
    {
        [CanBeNull] private readonly IQuerySource _querySource;
        private readonly IModel _model;

        /// <inheritdoc />
        public MongoDbEntityQueryableExpressionVisitor(
            // ReSharper disable once SuggestBaseTypeForParameter
            [NotNull] MongoDbEntityQueryModelVisitor entityQueryModelVisitor,
            [NotNull] IModel model,
            [CanBeNull] IQuerySource querySource)
            : base(entityQueryModelVisitor)
        {
            _model = Check.NotNull(model, nameof(model));
            _querySource = querySource;
        }

        private new MongoDbEntityQueryModelVisitor QueryModelVisitor => (MongoDbEntityQueryModelVisitor)base.QueryModelVisitor;

        /// <inheritdoc />
        protected override Expression VisitEntityQueryable(Type elementType)
        {
            Check.NotNull(elementType, nameof(elementType));

            IEntityType entityType = QueryModelVisitor.QueryCompilationContext.FindEntityType(_querySource)
                                     ?? _model.FindEntityType(elementType);
            MongoDbEntityTypeAnnotations annotations = entityType.MongoDb();

            while (annotations.IsDerivedType && entityType.BaseType != null)
            {
                entityType = entityType.BaseType;
                annotations = entityType.MongoDb();
            }

            return Expression.Call(
                entityType.ClrType == elementType
                    ? EntityQueryMethodInfo.MakeGenericMethod(elementType)
                    : SubEntityQueryMethodInfo.MakeGenericMethod(entityType.ClrType, elementType),
                EntityQueryModelVisitor.QueryContextParameter);
        }

        private static readonly MethodInfo EntityQueryMethodInfo
            = typeof(MongoDbEntityQueryableExpressionVisitor).GetTypeInfo()
                .GetDeclaredMethod(nameof(EntityQuery));

        [UsedImplicitly]
        private static IQueryable<TEntity> EntityQuery<TEntity>(
            QueryContext queryContext)
            where TEntity : class
            => ((MongoDbQueryContext)queryContext).MongoDbConnection
                .Query<TEntity>();

        private static readonly MethodInfo SubEntityQueryMethodInfo
            = typeof(MongoDbEntityQueryableExpressionVisitor).GetTypeInfo()
                .GetDeclaredMethod(nameof(SubEntityQuery));

        [UsedImplicitly]
        private static IQueryable<TEntity> SubEntityQuery<TBaseEntity, TEntity>(
            QueryContext queryContext)
            where TBaseEntity : class
            where TEntity : class, TBaseEntity
            => EntityQuery<TEntity>(queryContext)
                .OfType<TEntity>();
    }
}