using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using MongoDB.Driver;

namespace Blueshift.EntityFrameworkCore.MongoDB.Update
{
    /// <summary>
    /// Selects an instance of <see cref="IMongoDbWriteModelFactory{TEntity}"/> for a given <see cref="EntityType"/>.
    /// </summary>
    public class MongoDbWriteModelFactorySelector : IMongoDbWriteModelFactorySelector
    {
        private readonly IValueGeneratorSelector _valueGeneratorSelector;
        private readonly IMongoDbWriteModelFactoryCache _mongoDbWriteModelFactoryCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="IMongoDbWriteModelFactorySelector"/> class.
        /// </summary>
        /// <param name="valueGeneratorSelector">The <see cref="IValueGeneratorSelector"/> to use for populating concurrency tokens.</param>
        /// <param name="mongoDbWriteModelFactoryCache">A <see cref="IMongoDbWriteModelFactoryCache"/> that can be used to cache the
        /// factory instances returned by this <see cref="IMongoDbWriteModelFactorySelector"/>.</param>
        public MongoDbWriteModelFactorySelector(
            [NotNull] IValueGeneratorSelector valueGeneratorSelector,
            [NotNull] IMongoDbWriteModelFactoryCache mongoDbWriteModelFactoryCache)
        {
            _valueGeneratorSelector = Check.NotNull(valueGeneratorSelector, nameof(valueGeneratorSelector));
            _mongoDbWriteModelFactoryCache = Check.NotNull(mongoDbWriteModelFactoryCache, nameof(mongoDbWriteModelFactoryCache));
        }

        /// <summary>
        /// Select an <see cref="IMongoDbWriteModelFactory{TEntity}"/> instance for the given <paramref name="updateEntry"/>.
        /// </summary>
        /// <param name="updateEntry">The <see cref="IUpdateEntry"/> that the write model factory will be used to translate.</param>
        /// <typeparam name="TEntity">The type of entity for which to create a <see cref="IMongoDbWriteModelFactory{TEntity}"/> instance.</typeparam>
        /// <returns>An instance of <see cref="IMongoDbWriteModelFactory{TEntity}"/> that can be used to convert <see cref="IUpdateEntry"/>
        /// instances to <see cref="WriteModel{TDocument}"/> instances.</returns>
        public IMongoDbWriteModelFactory<TEntity> Select<TEntity>(IUpdateEntry updateEntry)
            => _mongoDbWriteModelFactoryCache.GetOrAdd(
                Check.NotNull(updateEntry, nameof(updateEntry)).EntityType,
                updateEntry.EntityState,
                Create<TEntity>);

        /// <summary>
        /// Creates a new instance of <see cref="IMongoDbWriteModelFactory{TEntity}"/> for the given <paramref name="entityType"/>
        /// and <paramref name="entityState"/>.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity for which to create a <see cref="IMongoDbWriteModelFactory{TEntity}"/> instance.</typeparam>
        /// <param name="entityType"></param>
        /// <param name="entityState"></param>
        /// <returns></returns>
        public IMongoDbWriteModelFactory<TEntity> Create<TEntity>(
            [NotNull] IEntityType entityType,
            EntityState entityState)
        {
            Check.NotNull(entityType, nameof(entityType));
            if (entityState != EntityState.Added &&
                entityState != EntityState.Modified &&
                entityState != EntityState.Deleted)
            {
                throw new InvalidOperationException($"The value provided for entityState must be Added, Modified, or Deleted, but was {entityState}.");
            }

            IMongoDbWriteModelFactory<TEntity> mongoDbWriteModelFactory;
            switch (entityState)
            {
                case EntityState.Added:
                    mongoDbWriteModelFactory = new InsertOneModelFactory<TEntity>(_valueGeneratorSelector, entityType);
                    break;
                case EntityState.Modified:
                    mongoDbWriteModelFactory = new UpdateOneModelFactory<TEntity>(_valueGeneratorSelector, entityType);
                    break;
                default:
                    mongoDbWriteModelFactory = new DeleteOneModelFactory<TEntity>(_valueGeneratorSelector, entityType);
                    break;
            }
            return mongoDbWriteModelFactory;
        }
    }
}