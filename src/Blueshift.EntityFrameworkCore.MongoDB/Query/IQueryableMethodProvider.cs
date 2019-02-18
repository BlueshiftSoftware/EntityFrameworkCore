using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public interface IQueryableMethodProvider
    {
        /// <inheritdoc cref="IQueryableMethodProvider" />
        MethodInfo All { get; }

        /// <inheritdoc cref="IQueryableMethodProvider" />
        MethodInfo Any { get; }

        /// <inheritdoc cref="IQueryableMethodProvider" />
        MethodInfo Cast { get; }

        /// <inheritdoc cref="IQueryableMethodProvider" />
        MethodInfo Concat { get; }

        /// <inheritdoc cref="IQueryableMethodProvider" />
        MethodInfo Contains { get; }

        /// <inheritdoc cref="IQueryableMethodProvider" />
        MethodInfo Count { get; }

        /// <inheritdoc cref="IQueryableMethodProvider" />
        MethodInfo DefaultIfEmpty { get; }

        /// <inheritdoc cref="IQueryableMethodProvider" />
        MethodInfo DefaultIfEmptyParameter { get; }

        /// <inheritdoc cref="IQueryableMethodProvider" />
        MethodInfo Distinct { get; }

        /// <inheritdoc cref="IQueryableMethodProvider" />
        MethodInfo Except { get; }

        /// <inheritdoc cref="IQueryableMethodProvider" />
        MethodInfo First { get; }

        /// <inheritdoc cref="IQueryableMethodProvider" />
        MethodInfo FirstOrDefault { get; }

        /// <inheritdoc cref="IQueryableMethodProvider" />
        MethodInfo GroupBy { get; }

        /// <inheritdoc cref="IQueryableMethodProvider" />
        MethodInfo GroupJoin { get; }

        /// <inheritdoc cref="IQueryableMethodProvider" />
        MethodInfo Intersect { get; }

        /// <inheritdoc cref="IQueryableMethodProvider" />
        MethodInfo Join { get; }

        /// <inheritdoc cref="IQueryableMethodProvider" />
        MethodInfo Last { get; }

        /// <inheritdoc cref="IQueryableMethodProvider" />
        MethodInfo LastOrDefault { get; }

        /// <inheritdoc cref="IQueryableMethodProvider" />
        MethodInfo LongCount { get; }

        /// <inheritdoc cref="IQueryableMethodProvider" />
        MethodInfo OfType { get; }

        /// <inheritdoc cref="IQueryableMethodProvider" />
        MethodInfo OrderBy { get; }

        /// <inheritdoc cref="IQueryableMethodProvider" />
        MethodInfo OrderByDescending { get; }

        /// <inheritdoc cref="IQueryableMethodProvider" />
        MethodInfo Select { get; }

        /// <inheritdoc cref="IQueryableMethodProvider" />
        MethodInfo SelectMany { get; }

        /// <inheritdoc cref="IQueryableMethodProvider" />
        MethodInfo Single { get; }

        /// <inheritdoc cref="IQueryableMethodProvider" />
        MethodInfo SingleOrDefault { get; }

        /// <inheritdoc cref="IQueryableMethodProvider" />
        MethodInfo Skip { get; }

        /// <inheritdoc cref="IQueryableMethodProvider" />
        MethodInfo Take { get; }

        /// <inheritdoc cref="IQueryableMethodProvider" />
        MethodInfo ThenBy { get; }

        /// <inheritdoc cref="IQueryableMethodProvider" />
        MethodInfo ThenByDescending { get; }

        /// <inheritdoc cref="IQueryableMethodProvider" />
        MethodInfo Union { get; }

        /// <inheritdoc cref="IQueryableMethodProvider" />
        MethodInfo Where { get; }

        /// <inheritdoc cref="IQueryableMethodProvider" />
        bool IsQueryableExpression(Expression expression);

        /// <inheritdoc cref="IQueryableMethodProvider" />
        MethodInfo GetAggregateMethod(string methodName, Type elementType);

        /// <inheritdoc cref="IQueryableMethodProvider"/>
        Expression CreateLambdaExpression(
            Expression bodyExpression,
            params ParameterExpression[] parameterExpressions);
    }
}
