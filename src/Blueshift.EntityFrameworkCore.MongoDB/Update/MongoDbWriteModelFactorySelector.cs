using System;
using System.Collections.Concurrent;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
        private readonly ConcurrentDictionary<IEntityType, object> _cache
            = new ConcurrentDictionary<IEntityType, object>();
        private readonly IValueGeneratorSelector _valueGeneratorSelector;

        /// <summary>
        /// Initializes a new instance of the <see cref="IMongoDbWriteModelFactorySelector"/> class.
        /// </summary>
        /// <param name="valueGeneratorSelector">The <see cref="IValueGeneratorSelector"/> to use for populating concurrency tokens.</param>
        public MongoDbWriteModelFactorySelector([NotNull] IValueGeneratorSelector valueGeneratorSelector)
        {
            _valueGeneratorSelector = Check.NotNull(valueGeneratorSelector, nameof(valueGeneratorSelector));
        }

        /// <summary>
        /// Creates an <see cref="IMongoDbWriteModelFactory{TEntity}"/> instance for the given <paramref name="entityType"/>.
        /// </summary>
        /// <param name="entityType">The <see cref="IEntityType"/> that the write model factory will be used for.</param>
        /// <typeparam name="TEntity">The type of entity for which to create a <see cref="IMongoDbWriteModelFactory{TEntity}"/> instance.</typeparam>
        /// <returns>An instance of <see cref="IMongoDbWriteModelFactory{TEntity}"/> that can be used to convert <see cref="IEntityType"/>
        /// update entries to <see cref="WriteModel{TDocument}"/> instances.</returns>
        public IMongoDbWriteModelFactory<TEntity> CreateFactory<TEntity>([NotNull] IEntityType entityType)
            => (IMongoDbWriteModelFactory<TEntity>)_cache.GetOrAdd(
                Check.NotNull(entityType, nameof(entityType)),
                new MongoDbWriteModelFactory<TEntity>(_valueGeneratorSelector, entityType));
    }
}
