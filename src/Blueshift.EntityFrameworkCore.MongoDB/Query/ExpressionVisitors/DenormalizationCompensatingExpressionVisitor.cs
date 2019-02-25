using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Remotion.Linq.Parsing;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query.ExpressionVisitors
{
    /// <inheritdoc />
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class DenormalizationCompensatingExpressionVisitor : RelinqExpressionVisitor
    {
        /// <inheritdoc />
        protected override Expression VisitBlock(BlockExpression node)
        {
            if (TryFindAddToCollectionExpression(node.Expressions, out MethodCallExpression methodCallExpression))
            {
                Expression callReplaceDenormalizedInstances = Expression.Call(
                    null,
                    RemoveDenormalizedInstancesMethodInfo,
                    methodCallExpression.Arguments);
                IList<Expression> newBlockExpressions = new List<Expression>(node.Expressions);
                newBlockExpressions.Add(callReplaceDenormalizedInstances);
                node = node.Update(node.Variables, newBlockExpressions);
            }
            return base.VisitBlock(node);
        }

        private bool TryFindAddToCollectionExpression(IReadOnlyList<Expression> expressions, out MethodCallExpression methodCallExpression)
        {
            methodCallExpression = null;

            foreach (Expression expression in expressions)
            {
                if (expression is MethodCallExpression candidateMethodCallExpression
                    && candidateMethodCallExpression.Method == AddToCollectionSnapshotMethodInfo)
                {
                    methodCallExpression = candidateMethodCallExpression;
                    break;
                }
            }

            return methodCallExpression != null;
        }

        private static void RemoveDenormalizedInstances(
            IStateManager stateManager,
            INavigation navigation,
            object entity,
            object instance)
        {
            InternalEntityEntry internalEntityEntry = stateManager.TryGetEntry(entity);
            internalEntityEntry.EnsureRelationshipSnapshot();

            INavigation inverse = navigation.FindInverse();
            IKey primaryKey = inverse.DeclaringEntityType.FindPrimaryKey();
            IProperty primaryKeyProperty = primaryKey.Properties.Single();
            object keyValue = primaryKeyProperty.GetGetter().GetClrValue(instance);

            IClrCollectionAccessor collectionAccessor = navigation.GetCollectionAccessor();
            ICollection list = (ICollection) collectionAccessor.GetOrCreate(entity);

            IList<object> toRemove = list
                .OfType<object>()
                .Where(item => (Equals(
                                    keyValue,
                                    primaryKeyProperty.GetGetter().GetClrValue(item))
                                && !ReferenceEquals(instance, item)))
                .ToList();

            foreach (object item in toRemove)
            {
                collectionAccessor.Remove(entity, item);
                internalEntityEntry.RemoveFromCollectionSnapshot(navigation, item);
            }
        }

        private static readonly MethodInfo RemoveDenormalizedInstancesMethodInfo
            = MethodHelper
                .GetMethodInfo(() => RemoveDenormalizedInstances(default, default, default, default));

        private static readonly MethodInfo AddToCollectionSnapshotMethodInfo
            = (MethodInfo) typeof(IncludeCompiler)
                .GetNestedType("IncludeLoadTreeNode", BindingFlags.NonPublic)
                ?.GetField("_addToCollectionSnapshotMethodInfo", BindingFlags.NonPublic | BindingFlags.Static)
                ?.GetValue(null);
    }
}
