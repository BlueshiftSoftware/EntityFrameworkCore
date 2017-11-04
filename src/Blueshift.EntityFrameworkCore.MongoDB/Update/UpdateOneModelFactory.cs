using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using MongoDB.Driver;

namespace Blueshift.EntityFrameworkCore.MongoDB.Update
{
    /// <summary>
    /// Creates <see cref="UpdateOneModel{TEntity}"/> from a given <see cref="IUpdateEntry"/>.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity being added.</typeparam>
    public class UpdateOneModelFactory<TEntity> : MongoDbWriteModelFactory<TEntity>
    {
        private static readonly MethodInfo SetMethodInfo = typeof(UpdateDefinitionBuilder<TEntity>)
            .GetTypeInfo()
            .GetMember(nameof(UpdateDefinitionBuilder<TEntity>.Set), MemberTypes.Method, BindingFlags.Public | BindingFlags.Instance)
            .Select(memberInfo => (MethodInfo)memberInfo)
            .First(memberInfo => memberInfo.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == 
                typeof(Expression<>))
            .GetGenericMethodDefinition();

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDbWriteModelFactory{TEntity}"/> class.
        /// </summary>
        /// <param name="valueGeneratorSelector">The <see cref="IValueGeneratorSelector"/> to use for populating concurrency tokens.</param>
        /// <param name="entityType">The <see cref="IEntityType"/> for which this <see cref="MongoDbWriteModelFactory{TDocument}"/> will be used.</param>
        public UpdateOneModelFactory(
            [NotNull] IValueGeneratorSelector valueGeneratorSelector,
            [NotNull] IEntityType entityType)
            : base(
                  Check.NotNull(valueGeneratorSelector, nameof(valueGeneratorSelector)),
                  Check.NotNull(entityType, nameof(entityType)))
        {
        }

        /// <summary>
        /// Creates an <see cref="UpdateOneModel{TEntity}"/> that maps the given <paramref name="updateEntry"/>.
        /// </summary>
        /// <param name="updateEntry">The <see cref="IUpdateEntry"/> to map.</param>
        /// <returns>A new <see cref="UpdateOneModel{TEntity}"/> containing the inserted values represented
        /// by <paramref name="updateEntry"/>.</returns>
        public override WriteModel<TEntity> CreateWriteModel(IUpdateEntry updateEntry)
        {
            InternalEntityEntry internalEntityEntry = Check.Is<InternalEntityEntry>(updateEntry, nameof(updateEntry));
            UpdateDbGeneratedProperties(internalEntityEntry);
            return new UpdateOneModel<TEntity>(
                GetLookupFilter(updateEntry),
                CreateUpdateDefinition(internalEntityEntry));
        }

        private UpdateDefinition<TEntity> CreateUpdateDefinition(InternalEntityEntry internalEntityEntry)
        {
            var builder = new UpdateDefinitionBuilder<TEntity>();
            IEntityType entityType = internalEntityEntry.EntityType;
            IList<UpdateDefinition<TEntity>> updateDefinitions = entityType
                .GetProperties()
                .Where(property => property.PropertyInfo != null && internalEntityEntry.IsModified(property))
                .Select(property => CreatePropertyUpdateDefinition(builder, internalEntityEntry, property))
                .ToList();
            return builder.Combine(updateDefinitions);
        }

        private UpdateDefinition<TEntity> CreatePropertyUpdateDefinition(
            UpdateDefinitionBuilder<TEntity> builder,
            IUpdateEntry entityEntry,
            IPropertyBase property)
            => (UpdateDefinition<TEntity>)SetMethodInfo
                .MakeGenericMethod(property.ClrType)
                .Invoke(
                    builder,
                    new []
                    {
                        CreateFieldDefintion(property),
                        entityEntry.GetCurrentValue(property)
                    });

        private object CreateFieldDefintion(IPropertyBase property)
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(TEntity), name: "entity");
            return Expression.Lambda(
                Expression.MakeMemberAccess(parameterExpression, property.PropertyInfo),
                parameterExpression);
        }
    }
}