using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query.ExpressionVisitors
{
    /// <inheritdoc />
    public class MongoDbMemberAccessBindingExpressionVisitor : MemberAccessBindingExpressionVisitor
    {
        [NotNull] private readonly IModel _model;

        /// <inheritdoc />
        public MongoDbMemberAccessBindingExpressionVisitor(
            QuerySourceMapping querySourceMapping,
            EntityQueryModelVisitor queryModelVisitor,
            bool inProjection)
            : base(querySourceMapping, queryModelVisitor, inProjection)
        {
            _model = queryModelVisitor.QueryCompilationContext.Model;
        }

        /// <inheritdoc />
        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
            => methodCallExpression.IsEFProperty()
                ? ConvertPropertyMethodToMemberAccess(methodCallExpression)
                : base.VisitMethodCall(methodCallExpression);

        private Expression ConvertPropertyMethodToMemberAccess(MethodCallExpression methodCallExpression)
        {
            var memberNameExpression = (ConstantExpression)methodCallExpression.Arguments[1];
            IProperty property = _model
                .FindEntityType(methodCallExpression.Arguments[0].Type.FullName)
                .FindProperty((string)memberNameExpression.Value);
            Expression entityExpression = methodCallExpression.Arguments[0];
            MemberExpression memberExpression = property.IsForeignKey()
                ? BindForeignKey(entityExpression, property.AsProperty().ForeignKeys[0])
                : BindMember(entityExpression, property);
            return Visit(Expression.Convert(memberExpression, methodCallExpression.Type));
        }

        private MemberExpression BindMember(Expression entityExpression, IProperty property)
            => Expression.MakeMemberAccess(
                entityExpression,
                property.GetMemberInfo(false, false));

        private MemberExpression BindForeignKey(Expression entityExpression, IForeignKey foreignKey)
        {
            INavigation navigation = entityExpression.Type == foreignKey.PrincipalEntityType.ClrType
                ? foreignKey.PrincipalToDependent
                : foreignKey.DependentToPrincipal;
            return Expression.MakeMemberAccess(
                Expression.MakeMemberAccess(
                    entityExpression,
                    navigation.GetMemberInfo(false, false)),
                foreignKey.PrincipalKey.Properties[0].GetMemberInfo(false, false));
        }
    }
}