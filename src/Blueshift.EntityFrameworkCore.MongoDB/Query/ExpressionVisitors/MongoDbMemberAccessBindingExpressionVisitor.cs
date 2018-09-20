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
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query.ExpressionVisitors
{
    /// <inheritdoc />
    public class MongoDbMemberAccessBindingExpressionVisitor : MemberAccessBindingExpressionVisitor
    {
        private readonly IModel _model;

        /// <inheritdoc />
        public MongoDbMemberAccessBindingExpressionVisitor(
            [NotNull] QuerySourceMapping querySourceMapping,
            [NotNull] MongoDbEntityQueryModelVisitor mongoDbEntityQueryModelVisitor,
            bool inProjection)
            : base(
                Check.NotNull(querySourceMapping, nameof(querySourceMapping)),
                Check.NotNull(mongoDbEntityQueryModelVisitor, nameof(mongoDbEntityQueryModelVisitor)),
                inProjection)
        {
            _model = mongoDbEntityQueryModelVisitor.QueryCompilationContext.Model;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            Expression newExpression = null;

            if (methodCallExpression.Method.IsEFPropertyMethod())
            {
                var newArguments
                    = VisitAndConvert(
                        new List<Expression>
                        {
                            methodCallExpression.Arguments[0],
                            methodCallExpression.Arguments[1]
                        }.AsReadOnly(),
                        nameof(VisitMethodCall));

                Expression targetExpression = newArguments[0];

                IEntityType entityType = _model
                    .FindEntityType(targetExpression.Type);
                IProperty property = entityType
                    .FindProperty((string)((ConstantExpression)newArguments[1]).Value);
                PropertyInfo propertyInfo = property.PropertyInfo;

                if (property.IsShadowProperty && property.IsForeignKey())
                {
                    IForeignKey foreignKey = property.AsProperty().ForeignKeys.Single();
                    INavigation navigation = foreignKey.PrincipalEntityType == entityType
                        ? foreignKey.PrincipalToDependent
                        : foreignKey.DependentToPrincipal;

                    targetExpression = Expression.MakeMemberAccess(targetExpression, navigation.PropertyInfo);

                    IEntityType targetEntityType = navigation.GetTargetType();
                    property = targetEntityType.FindPrimaryKey().Properties.Single();
                    propertyInfo = property.PropertyInfo;
                }

                newExpression = Expression.Convert(
                    Expression.MakeMemberAccess(targetExpression, propertyInfo),
                    typeof(Nullable<>).MakeGenericType(propertyInfo.PropertyType));
            }

            return newExpression ?? base.VisitMethodCall(methodCallExpression);
        }
    }
}