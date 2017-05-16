using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Update;
using MongoDB.Driver;

namespace Blueshift.EntityFrameworkCore.MongoDB.Update
{
    /// <summary>
    /// Interface for selecting an instance of <see cref="IMongoDbWriteModelFactory{TEntity}"/>.
    /// </summary>
    public interface IMongoDbWriteModelFactorySelector
    {
        /// <summary>
        /// Select an <see cref="IMongoDbWriteModelFactory{TEntity}"/> instance for the given <paramref name="updateEntry"/>.
        /// </summary>
        /// <param name="updateEntry">The <see cref="IUpdateEntry"/> that the write model factory will be used to translate.</param>
        /// <typeparam name="TEntity">The type of entity for which to create a <see cref="IMongoDbWriteModelFactory{TEntity}"/> instance.</typeparam>
        /// <returns>An instance of <see cref="IMongoDbWriteModelFactory{TEntity}"/> that can be used to convert <see cref="IUpdateEntry"/>
        /// instances to <see cref="WriteModel{TDocument}"/> instances.</returns>
        IMongoDbWriteModelFactory<TEntity> Select<TEntity>([NotNull] IUpdateEntry updateEntry);
    }
}