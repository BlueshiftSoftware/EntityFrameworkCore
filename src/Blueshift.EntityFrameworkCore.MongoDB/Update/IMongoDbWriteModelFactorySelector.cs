using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using MongoDB.Driver;

namespace Blueshift.EntityFrameworkCore.MongoDB.Update
{
    /// <summary>
    /// Interface for selecting an instance of <see cref="IMongoDbWriteModelFactory{TEntity}"/>.
    /// </summary>
    public interface IMongoDbWriteModelFactorySelector
    {
        /// <summary>
        /// Creates an <see cref="IMongoDbWriteModelFactory{TEntity}"/> instance for the given <paramref name="entityType"/>.
        /// </summary>
        /// <param name="entityType">The <see cref="IEntityType"/> that the write model factory will be used for.</param>
        /// <typeparam name="TEntity">The type of entity for which to create a <see cref="IMongoDbWriteModelFactory{TEntity}"/> instance.</typeparam>
        /// <returns>An instance of <see cref="IMongoDbWriteModelFactory{TEntity}"/> that can be used to convert <see cref="IEntityType"/>
        /// update entries to <see cref="WriteModel{TDocument}"/> instances.</returns>
        IMongoDbWriteModelFactory<TEntity> CreateFactory<TEntity>([NotNull] IEntityType entityType);
    }
}