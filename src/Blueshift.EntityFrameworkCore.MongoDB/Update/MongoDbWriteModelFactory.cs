using System;
using System.Collections.Concurrent;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using MongoDB.Driver;

namespace Blueshift.EntityFrameworkCore.MongoDB.Update
{
    /// <summary>
    /// Generates <see cref="WriteModel{TEntity}"/> instances from <see cref="IUpdateEntry"/> instances.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity being updated</typeparam>
    public class MongoDbWriteModelFactory<TEntity> : IMongoDbWriteModelFactory<TEntity>
    {
        private readonly ConcurrentDictionary<EntityState, IWriteModelGenerator<TEntity>> _cache
            = new ConcurrentDictionary<EntityState, IWriteModelGenerator<TEntity>>();

        private readonly IValueGeneratorSelector _valueGeneratorSelector;
        private readonly IEntityType _entityType;

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDbWriteModelFactory{TEntity}"/> class.
        /// </summary>
        /// <param name="valueGeneratorSelector">The <see cref="IValueGeneratorSelector"/> to use for populating concurrency tokens.</param>
        /// <param name="entityType">The <see cref="IEntityType"/> for which this <see cref="MongoDbWriteModelFactory{TDocument}"/> will be used.</param>
        public MongoDbWriteModelFactory(
            [NotNull] IValueGeneratorSelector valueGeneratorSelector,
            [NotNull] IEntityType entityType)
        {
            _valueGeneratorSelector = Check.NotNull(valueGeneratorSelector, nameof(valueGeneratorSelector));
            _entityType = Check.NotNull(entityType, nameof(entityType));
        }

        /// <summary>
        /// Converts an <see cref="IUpdateEntry"/> instance to a <see cref="WriteModel{TEntity}"/> instance.
        /// </summary>
        /// <param name="updateEntry">The <see cref="IUpdateEntry"/> entry to convert.</param>
        /// <returns>A new <see cref="WriteModel{TEntity}"/> that contains the updates in <see cref="IUpdateEntry"/>.</returns>
        public WriteModel<TEntity> CreateWriteModel([NotNull] IUpdateEntry updateEntry)
        {
            Check.NotNull(updateEntry, nameof(updateEntry));
            if (!typeof(TEntity).GetTypeInfo().IsAssignableFrom(updateEntry.EntityType.ClrType))
            {
                throw new InvalidOperationException($"Entity must derive from {nameof(TEntity)}.");
            }
            if (updateEntry.EntityState != EntityState.Added &&
                updateEntry.EntityState != EntityState.Modified &&
                updateEntry.EntityState != EntityState.Deleted)
            {
                throw new InvalidOperationException($"Entity state must be Added, Modified, or Deleted.");
            }

            IWriteModelGenerator<TEntity> writeModelGenerator = _cache.GetOrAdd(
                updateEntry.EntityState,
                CreateWriteModelGenerator);
            return writeModelGenerator.GenerateWriteModel(updateEntry);
        }

        private IWriteModelGenerator<TEntity> CreateWriteModelGenerator(EntityState entityState)
        {
            IWriteModelGenerator<TEntity> writeModelGenerator;
            switch (entityState)
            {
                case EntityState.Added:
                    writeModelGenerator = new InsertOneWriteModelGenerator<TEntity>(_valueGeneratorSelector, _entityType);
                    break;
                case EntityState.Modified:
                    writeModelGenerator = new UpdateOneWriteModelGenerator<TEntity>(_valueGeneratorSelector, _entityType);
                    break;
                default:
                    writeModelGenerator = new DeleteOneWriteModelGenerator<TEntity>(_valueGeneratorSelector, _entityType);
                    break;
            }
            return writeModelGenerator;
        }
    }
}