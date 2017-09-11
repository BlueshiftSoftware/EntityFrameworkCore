using System;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Driver;

namespace Blueshift.EntityFrameworkCore.MongoDB.Update
{
    /// <summary>
    /// Caches <see cref="IMongoDbWriteModelFactory{TEntity}"/> instances.
    /// </summary>
    public class MongoDbWriteModelFactoryCache : IMongoDbWriteModelFactoryCache
    {
        private readonly ConcurrentDictionary<CacheKey, object> _cache
            = new ConcurrentDictionary<CacheKey, object>();

        /// <summary>
        /// Returns a cached or newly created instance of <see cref="IMongoDbWriteModelFactory{TEntity}"/> for the given 
        /// <paramref name="entityType"/> and <paramref name="entityState"/>.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity being written.</typeparam>
        /// <param name="entityType">The <see cref="IEntityType"/> that contains the entity metadata.</param>
        /// <param name="entityState">The <see cref="EntityState"/> describing the type of <see cref="WriteModel{TEntity}"/>
        /// that returned the factory will produce.</param>
        /// <param name="factoryFunc">A <see cref="Func{IEntityType, EntityState, IMongoDbWriteModelFactory}"/> that can
        /// be used to create a new factory instance if one has not previously been cached.</param>
        /// <returns>A new or cached instance of <see cref="IMongoDbWriteModelFactory{TEntity}"/>.</returns>
        public IMongoDbWriteModelFactory<TEntity> GetOrAdd<TEntity>(
            IEntityType entityType,
            EntityState entityState,
            Func<IEntityType, EntityState, IMongoDbWriteModelFactory<TEntity>> factoryFunc)
            => _cache.GetOrAdd(
                    new CacheKey(
                        Check.NotNull(entityType, nameof(entityType)),
                        entityState,
                        Check.NotNull(factoryFunc, nameof(factoryFunc))),
                    cacheKey => cacheKey.FactoryFunc(entityType, entityState))
                as IMongoDbWriteModelFactory<TEntity>;

        private struct CacheKey : IEquatable<CacheKey>
        {
            private readonly IEntityType _entityType;
            private readonly EntityState _entityState;

            public CacheKey(
                IEntityType entityType,
                EntityState entityState,
                Func<IEntityType, EntityState, object> factoryFunc)
            {
                _entityType = entityType;
                _entityState = entityState;
                FactoryFunc = factoryFunc;
            }

            public Func<IEntityType, EntityState, object> FactoryFunc { get; }

            public override bool Equals(object obj)
                => Equals((CacheKey)obj);

            public bool Equals(CacheKey other)
                => Equals(_entityType, other._entityType)
                    && Equals(_entityState, other._entityState);

            public override int GetHashCode()
            {
                unchecked
                {
                    return (_entityType.GetHashCode() * 492) ^ _entityState.GetHashCode();
                }
            }
        }
    }
}