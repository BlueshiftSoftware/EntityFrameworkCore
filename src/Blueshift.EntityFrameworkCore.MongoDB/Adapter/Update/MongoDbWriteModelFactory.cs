using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using MongoDB.Driver;

namespace Blueshift.EntityFrameworkCore.MongoDB.Adapter.Update
{
    /// <summary>
    /// Base class for generating <see cref="WriteModel{TEntity}"/> instances.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to write.</typeparam>
    public abstract class MongoDbWriteModelFactory<TEntity> : IMongoDbWriteModelFactory<TEntity>
    {
        private static readonly MethodInfo GenericEqMethodInfo = MethodHelper
            .GetGenericMethodDefinition<FilterDefinitionBuilder<TEntity>, object>(
                filterDefinitionBuilder => filterDefinitionBuilder.Eq(
                    (Expression<Func<TEntity, object>>) null,
                    null));

        private readonly IValueGeneratorSelector _valueGeneratorSelector;
        private readonly IEnumerable<IProperty> _keyProperties;
        private readonly IEnumerable<IProperty> _concurrencyProperties;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDbWriteModelFactory{TEntity}"/> class.
        /// </summary>
        /// <param name="valueGeneratorSelector">The <see cref="IValueGeneratorSelector"/> to use for populating concurrency tokens.</param>
        /// <param name="entityType">The <see cref="IEntityType"/> for which this <see cref="MongoDbWriteModelFactory{TDocument}"/> will be used.</param>
        protected MongoDbWriteModelFactory(
            [NotNull] IValueGeneratorSelector valueGeneratorSelector,
            [NotNull] IEntityType entityType)
        {
            _valueGeneratorSelector = Check.NotNull(valueGeneratorSelector, nameof(valueGeneratorSelector));
            _keyProperties = Check.NotNull(entityType, nameof(entityType))
                .FindPrimaryKey()
                .Properties;
            _concurrencyProperties = entityType
                .GetProperties()
                .Where(property => property.IsConcurrencyToken)
                .ToList();
        }

        /// <summary>
        /// Converts an <see cref="IUpdateEntry"/> instance to a <see cref="WriteModel{TEntity}"/> instance.
        /// </summary>
        /// <param name="updateEntry">The <see cref="IUpdateEntry"/> entry to convert.</param>
        /// <returns>A new <see cref="WriteModel{TEntity}"/> that contains the updates in <see cref="IUpdateEntry"/>.</returns>
        public abstract WriteModel<TEntity> CreateWriteModel(IUpdateEntry updateEntry);

        /// <summary>
        /// Generates a <see cref="FilterDefinition{TEntity}"/> for <paramref name="updateEntry"/>.
        /// </summary>
        /// <param name="updateEntry">The <see cref="IUpdateEntry"/> for the document being updated.</param>
        /// <returns>A new <see cref="FilterDefinition{TEntity}"/> that can matches the document in <paramref name="updateEntry"/>.</returns>
        protected FilterDefinition<TEntity> GetLookupFilter([NotNull] IUpdateEntry updateEntry)
        {
            IEnumerable<IProperty> lookupProperties =
                Check.NotNull(updateEntry, nameof(updateEntry)).EntityState == EntityState.Added
                    ? _keyProperties
                    : _keyProperties.Concat(_concurrencyProperties);
            IList<FilterDefinition<TEntity>> filterDefinitions = lookupProperties
                .Select(property => GetPropertyFilterDefinition(
                    property,
                    property.IsConcurrencyToken
                        ? updateEntry.GetOriginalValue(property)
                        : updateEntry.GetCurrentValue(property)))
                .DefaultIfEmpty(Builders<TEntity>.Filter.Empty)
                .ToList();
            return filterDefinitions.Count > 1
                ? Builders<TEntity>.Filter.And(filterDefinitions)
                : filterDefinitions[0];
        }

        /// <summary>
        /// Updates the concurrency properties for <paramref name="internalEntityEntry"/>.
        /// </summary>
        /// <param name="internalEntityEntry">The <see cref="IUpdateEntry"/> the <see cref="IUpdateEntry"/> representing the document being updated.</param>
        protected void UpdateConcurrencyProperties(InternalEntityEntry internalEntityEntry)
        {
            var entityEntry = internalEntityEntry.ToEntityEntry();
            foreach (IProperty property in _concurrencyProperties)
            {
                ValueGenerator valueGenerator = _valueGeneratorSelector.Select(property, internalEntityEntry.EntityType);
                object dbGeneratedValue = valueGenerator.Next(entityEntry);
                internalEntityEntry.SetProperty(property, dbGeneratedValue, true);
                property.GetSetter().SetClrValue(internalEntityEntry.Entity, dbGeneratedValue);
            }
        }

        private FilterDefinition<TEntity> GetPropertyFilterDefinition(
            IPropertyBase property,
            object propertyValue)
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(TEntity), name: "entity");
            LambdaExpression lambdaExpression = Expression.Lambda(
                Expression.MakeMemberAccess(parameterExpression, property.PropertyInfo),
                parameterExpression);
            return (FilterDefinition<TEntity>)GenericEqMethodInfo
                .MakeGenericMethod(property.ClrType)
                .Invoke(Builders<TEntity>.Filter, new[] { lambdaExpression, propertyValue });
        }
    }
}