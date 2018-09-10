using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Blueshift.EntityFrameworkCore.MongoDB
{
    /// <summary>
    /// Provides utility methods for retrieving method info.
    /// </summary>
    public static class MethodHelper
    {
        /// <summary>
        /// Gets a generic method definition from a given delegate.
        /// </summary>
        /// <param name="delegateExpression">An expression representing a given method call.</param>
        /// <returns>The generic method defintion for the method in <paramref name="delegateExpression"/></returns>
        public static MethodInfo GetMethodInfo(Expression<Action> delegateExpression)
            => ((MethodCallExpression)delegateExpression.Body)
                .Method;

        /// <summary>
        /// Gets a generic method definition from a given delegate.
        /// </summary>
        /// <typeparam name="TInstance">The type of the delegate's target.</typeparam>
        /// <param name="delegateExpression">An expression representing a given method call.</param>
        /// <returns>The generic method defintion for the method in <paramref name="delegateExpression"/></returns>
        public static MethodInfo GetMethodInfo<TInstance>(Expression<Action<TInstance>> delegateExpression)
            => ((MethodCallExpression)delegateExpression.Body)
                .Method;

        /// <summary>
        /// Gets a generic method definition from a given delegate.
        /// </summary>
        /// <param name="delegateExpression">An expression representing a given method call.</param>
        /// <returns>The generic method defintion for the method in <paramref name="delegateExpression"/></returns>
        public static MethodInfo GetGenericMethodDefinition(Expression<Action> delegateExpression)
            => ((MethodCallExpression)delegateExpression.Body)
                .Method
                .GetGenericMethodDefinition();

        /// <summary>
        /// Gets a generic method definition from a given delegate.
        /// </summary>
        /// <typeparam name="T">The type of the delegate's return value.</typeparam>
        /// <param name="delegateExpression">An expression representing a given method call.</param>
        /// <returns>The generic method defintion for the method in <paramref name="delegateExpression"/></returns>
        public static MethodInfo GetGenericMethodDefinition<T>(Expression<Func<T>> delegateExpression)
            => ((MethodCallExpression)delegateExpression.Body)
                .Method
                .GetGenericMethodDefinition();

        /// <summary>
        /// Gets a generic method definition from a given delegate.
        /// </summary>
        /// <typeparam name="TIn">The type of item on which the delegate is called.</typeparam>
        /// <typeparam name="TOut">The type of the delegate's return value.</typeparam>
        /// <param name="delegateExpression">An expression representing a given method call.</param>
        /// <returns>The generic method defintion for the method in <paramref name="delegateExpression"/></returns>
        public static MethodInfo GetGenericMethodDefinition<TIn, TOut>(Expression<Func<TIn, TOut>> delegateExpression)
            => ((MethodCallExpression)delegateExpression.Body)
                .Method
                .GetGenericMethodDefinition();
    }
}
