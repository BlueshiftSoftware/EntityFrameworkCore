using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Parsing;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query.ExpressionVisitors
{
    /// <inheritdoc />
    public class RewritingExpressionVisitor : RelinqExpressionVisitor
    {
        [NotNull] private readonly IQueryableMethodProvider _queryableMethodProvider;
        private readonly QueryableMethodScope _queryableMethodScope;

        /// <inheritdoc />
        public RewritingExpressionVisitor(
            [NotNull] IQueryableMethodProvider queryableMethodProvider)
        {
            _queryableMethodProvider = Check.NotNull(queryableMethodProvider, nameof(queryableMethodProvider));
            _queryableMethodScope = new QueryableMethodScope();
        }

        /// <inheritdoc />
        protected override Expression VisitExtension(Expression node)
            => _queryableMethodScope.InQueryableMethod
               && node is NullConditionalExpression nullConditionalExpression
                ? Visit(nullConditionalExpression.AccessOperation)
                : base.VisitExtension(node);

        /// <inheritdoc />
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            Expression newBody = Visit(node.Body);

            Expression newExpression = newBody.Type != node.ReturnType
                ? _queryableMethodProvider.CreateLambdaExpression(
                    newBody,
                    node.Parameters)
                : newBody != node.Body
                    ? node.Update(newBody, node.Parameters)
                    : node;

            return newExpression;
        }

        /// <inheritdoc />
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            bool inQueryableMethod = _queryableMethodScope.InQueryableMethod ||
                                     _queryableMethodProvider.IsQueryableExpression(node);

            using (_queryableMethodScope.PushContext(inQueryableMethod))
            {
                IList<Expression> newArguments = new List<Expression>();

                Expression target = Visit(node.Object);
                bool modified = target != node.Object;

                foreach (Expression argument in node.Arguments)
                {
                    Expression newArgument = Visit(argument);
                    modified |= newArgument != argument;

                    newArguments.Add(newArgument);
                }

                return modified
                    ? _queryableMethodProvider.UpdateMethodCallExpression(node, target, newArguments)
                    : node;
            }
        }

        /// <inheritdoc />
        protected override Expression VisitUnary(UnaryExpression node)
        {
            Expression newExpression = Visit(node.Operand);

            if (!_queryableMethodScope.InQueryableMethod
                || !node.IsLiftedToNull
                || (node.NodeType != ExpressionType.Convert
                    && node.NodeType != ExpressionType.ConvertChecked
                    && node.NodeType != ExpressionType.TypeAs))
            {
                newExpression = newExpression != node.Operand
                    ? node.Update(newExpression)
                    : node;
            }

            return newExpression;
        }
    }
}
