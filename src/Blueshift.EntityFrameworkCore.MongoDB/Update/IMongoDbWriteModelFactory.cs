using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Update;
using MongoDB.Driver;

namespace Blueshift.EntityFrameworkCore.MongoDB.Update
{
    /// <summary>
    /// Interface for generating <see cref="WriteModel{TEntity}"/> instances from <see cref="IUpdateEntry"/> instances.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity being updated</typeparam>
    public interface IMongoDbWriteModelFactory<TEntity>
    {
        /// <summary>
        /// Converts an <see cref="IUpdateEntry"/> instance to a <see cref="WriteModel{TEntity}"/> instance.
        /// </summary>
        /// <param name="updateEntry">The <see cref="IUpdateEntry"/> entry to convert.</param>
        /// <returns>A new <see cref="WriteModel{TEntity}"/> that contains the updates in <see cref="IUpdateEntry"/>.</returns>
        WriteModel<TEntity> CreateWriteModel([NotNull] IUpdateEntry updateEntry);
    }
}