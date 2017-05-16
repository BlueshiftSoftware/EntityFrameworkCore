using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using MongoDB.Driver;

namespace Blueshift.EntityFrameworkCore.MongoDB.Update
{
    /// <summary>
    /// Caches <see cref="IMongoDbWriteModelFactory{TEntity}"/> instances.
    /// </summary>
    public interface IMongoDbWriteModelFactoryCache
    {
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
        IMongoDbWriteModelFactory<TEntity> GetOrAdd<TEntity>(
            IEntityType entityType,
            EntityState entityState,
            Func<IEntityType, EntityState, IMongoDbWriteModelFactory<TEntity>> factoryFunc);
    }
}