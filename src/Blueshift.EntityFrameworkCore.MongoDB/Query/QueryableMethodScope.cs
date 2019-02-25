using System;
using System.Linq;
using System.Linq.Expressions;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query
{
    /// <inheritdoc />
    /// <summary>
    /// Tracks whether or not an <see cref="ExpressionVisitor"/> is inside a call to a <see cref="Queryable"/> method.
    /// </summary>
    public class QueryableMethodScope : ExpressionVisitorScope<QueryableMethodScope.QueryableMethodContext>
    {
        /// <summary>
        /// Gets a value indicating whether or not the current <see cref="ExpressionVisitor"/> is inside a call to a <see cref="Queryable"/> method.
        /// </summary>
        public bool InQueryableMethod
            => CurrentContext?.InQueryableMethod ?? false;

        /// <summary>
        /// Pushes a new scope stack frame for the current <see cref="ExpressionVisitor"/>.
        /// </summary>
        /// <param name="inQueryableMethod"><c>true</c> if the current <see cref="ExpressionVisitor"/> is inside a <see cref="MethodCallExpression"/>
        /// representing a <see cref="Queryable"/> method; otherwise <c>false</c>.</param>
        /// <returns>A new <see cref="ExpressionVisitorScope{TCotext}.ExpressionVisitorScopeFrame"/> that pops the current stack
        /// frame when disposed.</returns>
        public ExpressionVisitorScopeFrame PushContext(bool inQueryableMethod)
            => PushContext(new QueryableMethodContext(inQueryableMethod));

        /// <summary>
        /// Describes whether or not a expression visitor is currently inside a <see cref="MethodCallExpression"/> expression sub-tree that
        /// represents a <see cref="Queryable"/> method.
        /// </summary>
        public class QueryableMethodContext
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="QueryableMethodContext"/> 
            /// </summary>
            /// <param name="inQueryableMethod"><c>true</c> if the current context represents a <see cref="Queryable"/> method; otherwise <c>false</c>.</param>
            public QueryableMethodContext(bool inQueryableMethod)
            {
                InQueryableMethod = inQueryableMethod;
            }

            /// <summary>
            /// Gets a value indicating whether or not the current expression tree is inside a <see cref="MethodCallExpression"/>
            /// referencing a <see cref="Queryable"/> method.
            /// </summary>
            public bool InQueryableMethod { get; }
        }
    }
}
