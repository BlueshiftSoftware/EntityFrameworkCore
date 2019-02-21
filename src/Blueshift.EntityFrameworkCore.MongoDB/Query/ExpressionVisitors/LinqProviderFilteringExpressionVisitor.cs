using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Parsing;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query.ExpressionVisitors
{
    /// <inheritdoc />
    public class LinqProviderFilteringExpressionVisitor : RelinqExpressionVisitor
    {
        [NotNull] private readonly IQueryableMethodProvider _queryableMethodProvider;
        private readonly ExpressionTreeScopeManager _expressionTreeScopeManager;

        /// <inheritdoc />
        public LinqProviderFilteringExpressionVisitor(
            [NotNull] IQueryableMethodProvider queryableMethodProvider)
        {
            _queryableMethodProvider = Check.NotNull(queryableMethodProvider, nameof(queryableMethodProvider));
            _expressionTreeScopeManager = new ExpressionTreeScopeManager();
        }

        /// <inheritdoc />
        protected override Expression VisitExtension(Expression node)
            => _expressionTreeScopeManager.InQueryableMethod
               && node is NullConditionalExpression nullConditionalExpression
                ? Visit(nullConditionalExpression.AccessOperation)
                : base.VisitExtension(node);

        /// <inheritdoc />
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            bool inQueryableMethod = _expressionTreeScopeManager.InQueryableMethod ||
                                     _queryableMethodProvider.IsQueryableExpression(node);

            using (_expressionTreeScopeManager.BeginScope(inQueryableMethod))
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

        /// <inheritdoc cref="LinqProviderFilteringExpressionVisitor" />
        protected class ExpressionTreeScopeManager
        {
            private readonly Stack<ExpressionTreeScope> _scopeStack;

            /// <inheritdoc cref="LinqProviderFilteringExpressionVisitor" />
            public ExpressionTreeScopeManager()
            {
                _scopeStack = new Stack<ExpressionTreeScope>();
            }

            /// <inheritdoc cref="LinqProviderFilteringExpressionVisitor" />
            public bool InQueryableMethod
                => _scopeStack.Count > 0
                   && (_scopeStack.Peek()?.InQueryableMethod ?? false);

            /// <inheritdoc cref="LinqProviderFilteringExpressionVisitor" />
            public ExpressionTreeScope BeginScope(bool inQueryableMethod)
            {
                ExpressionTreeScope expressionTreeScope = new ExpressionTreeScope(inQueryableMethod, this);
                _scopeStack.Push(expressionTreeScope);
                return expressionTreeScope;
            }

            /// <inheritdoc cref="LinqProviderFilteringExpressionVisitor" />
            public bool PopScope(ExpressionTreeScope expressionTreeScope)
            {
                bool popped = false;

                if (expressionTreeScope == _scopeStack.Peek())
                {
                    _scopeStack.Pop();
                    popped = true;
                }

                return popped;
            }
        }

        /// <inheritdoc cref="LinqProviderFilteringExpressionVisitor" />
        protected class ExpressionTreeScope : IDisposable
        {
            private readonly ExpressionTreeScopeManager _expressionTreeScopeManager;

            private bool _disposed;

            /// <inheritdoc cref="LinqProviderFilteringExpressionVisitor" />
            public ExpressionTreeScope(
                bool inQueryableMethod,
                ExpressionTreeScopeManager expressionTreeScopeManager)
            {
                InQueryableMethod = inQueryableMethod;
                _expressionTreeScopeManager = expressionTreeScopeManager;
            }

            /// <inheritdoc cref="LinqProviderFilteringExpressionVisitor" />
            public bool InQueryableMethod { get; }

            /// <inheritdoc cref="LinqProviderFilteringExpressionVisitor" />
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <inheritdoc cref="LinqProviderFilteringExpressionVisitor" />
            protected virtual void Dispose(bool disposing)
            {
                if (disposing && !_disposed)
                {
                    _expressionTreeScopeManager.PopScope(this);
                    _disposed = true;
                }
            }
        }
    }
}
