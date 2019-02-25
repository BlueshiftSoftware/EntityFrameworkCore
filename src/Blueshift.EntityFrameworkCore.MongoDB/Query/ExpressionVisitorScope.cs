using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query
{
    /// <summary>
    /// Manages an expression tree context scope for an <see cref="T:System.Linq.Expressions.ExpressionVisitor" />.
    /// </summary>
    /// <typeparam name="TContext">The type of object used to describe the expression tree context.</typeparam>
    public class ExpressionVisitorScope<TContext>
        where TContext : class
    {
        private readonly Stack<TContext> _scopeStack;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionVisitorScope{TContext}"/> class.
        /// </summary>
        public ExpressionVisitorScope()
        {
            _scopeStack = new Stack<TContext>();
        }

        /// <summary>
        /// Gets the <typeparamref name="TContext"/> for the current scope, or <c>null</c> if no scope exists yet.
        /// </summary>
        public TContext CurrentContext
            => _scopeStack.Count > 0
                ? _scopeStack.Peek()
                : null;

        /// <summary>
        /// Pushes an instance of <typeparamref name="TContext"/> onto the current stack.
        /// </summary>
        /// <param name="scopeContext">The <typeparamref name="TContext"/> instance used to describe the current scope.</param>
        /// <returns>A disposable <see cref="ExpressionVisitorScopeFrame"/> that pops the provided context off the stack when disposed.</returns>
        public ExpressionVisitorScopeFrame PushContext(TContext scopeContext)
        {
            _scopeStack.Push(scopeContext);
            return new ExpressionVisitorScopeFrame(this);
        }

        private void PopContext()
        {
            _scopeStack.Pop();
        }

        /// <inheritdoc />
        /// <summary>
        /// Represents a stack frame in the current <see cref="ExpressionVisitorScope{TContext}"/>.
        /// </summary>
        public class ExpressionVisitorScopeFrame : IDisposable
        {
            private readonly ExpressionVisitorScope<TContext> _expressionVisitorScope;
            private bool _disposed;

            /// <summary>
            /// Initializes a new instance of the <see cref="ExpressionVisitorScopeFrame"/> class.
            /// </summary>
            /// <param name="expressionVisitorScope">The <see cref="ExpressionVisitorScope{TContext}"/> that owns the current frame.</param>
            public ExpressionVisitorScopeFrame([NotNull] ExpressionVisitorScope<TContext> expressionVisitorScope)
            {
                _expressionVisitorScope = Check.NotNull(expressionVisitorScope, nameof(expressionVisitorScope));
            }

            /// <summary>
            /// Disposes the current <see cref="ExpressionVisitorScopeFrame"/> and returns the previous frame to the top of the scope stack.
            /// </summary>
            /// <param name="disposing"><c>true</c> if this method was called from <see cref="Dispose()"/>; otherwise <c>false</c>.</param>
            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (!_disposed)
                    {
                        _expressionVisitorScope.PopContext();
                        _disposed = true;
                    }
                }
            }

            /// <inheritdoc />
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }
    }
}
