using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq;
using Remotion.Linq.Clauses;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query
{
    /// <inheritdoc />
    public class MongoDbEntityQueryModelVisitor : EntityQueryModelVisitor
    {
        /// <inheritdoc />
        public MongoDbEntityQueryModelVisitor(
            [NotNull] EntityQueryModelVisitorDependencies entityQueryModelVisitorDependencies,
            [NotNull] QueryCompilationContext queryCompilationContext)
            : base(
                Check.NotNull(entityQueryModelVisitorDependencies, nameof(entityQueryModelVisitorDependencies)),
                Check.NotNull(queryCompilationContext, nameof(queryCompilationContext))
            )
        {
        }

        /// <inheritdoc />
        public override void VisitGroupJoinClause(GroupJoinClause groupJoinClause, QueryModel queryModel, int index)
        {
            base.VisitGroupJoinClause(groupJoinClause, queryModel, index);

            if (Expression is MethodCallExpression methodCallExpression
                && methodCallExpression.Method.MethodIsClosedFormOf(LinqOperatorProvider.GroupJoin))
            {
                Type outerType = methodCallExpression.Arguments[0].Type.TryGetSequenceType()
                                 ?? methodCallExpression.Arguments[0].Type;
                Type innerType = methodCallExpression.Arguments[1].Type.TryGetSequenceType()
                                 ?? methodCallExpression.Arguments[1].Type;

                Expression = Expression.Call(
                    methodCallExpression.Method,
                    Expression.Call(
                        BufferEntitiesMethodInfo.MakeGenericMethod(outerType),
                        methodCallExpression.Arguments[0],
                        Expression.Constant(QueryCompilationContext.Model),
                        QueryContextParameter,
                        Expression.Constant(QueryCompilationContext.IsTrackingQuery)),
                    Expression.Call(
                        BufferEntitiesMethodInfo.MakeGenericMethod(innerType),
                        methodCallExpression.Arguments[1],
                        Expression.Constant(QueryCompilationContext.Model),
                        QueryContextParameter,
                        Expression.Constant(QueryCompilationContext.IsTrackingQuery)),
                    methodCallExpression.Arguments[2],
                    methodCallExpression.Arguments[3],
                    methodCallExpression.Arguments[4]);
            }
        }

        private static readonly MethodInfo BufferEntitiesMethodInfo = typeof(MongoDbEntityQueryModelVisitor)
            .GetTypeInfo()
            .GetDeclaredMethod(nameof(BufferEntities));

        private static IEnumerable<TEntity> BufferEntities<TEntity>(
            IEnumerable<TEntity> enumerable,
            IModel model,
            QueryContext queryContext,
            bool queryStateManager)
        {
            foreach (TEntity entity in enumerable)
            {
                IEntityType entityType = model
                    .FindEntityType(entity.GetType());
                IProperty[] properties = entityType
                    .GetProperties()
                    .ToArray();

                ValueBuffer valueBuffer = properties
                    .Where(property => !property.IsShadowProperty)
                    .Aggregate(
                        new object[properties.Length],
                        (values, property) =>
                        {
                            values[property.GetIndex()] = property.GetGetter().GetClrValue(entity);
                            return values;
                        },
                        values => new ValueBuffer(values));

                yield return (TEntity) queryContext.QueryBuffer
                    .GetEntity(
                        entityType.FindPrimaryKey(),
                        new EntityLoadInfo(
                            valueBuffer,
                            vr => entity),
                        queryStateManager,
                        throwOnNullKey: false);
            }
        }
    }
}
