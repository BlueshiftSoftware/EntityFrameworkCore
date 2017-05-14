using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Update;
using MongoDB.Driver;

namespace Blueshift.EntityFrameworkCore.MongoDB.Update
{
    /// <summary>
    /// Generates a <see cref="WriteModel{TEntity}"/> given an <see cref="IUpdateEntry"/>.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity being written.</typeparam>
    public interface IWriteModelGenerator<TEntity>
    {
        /// <summary>
        /// Creates a <see cref="WriteModel{TEntity}"/> that maps the given <paramref name="updateEntry"/>.
        /// </summary>
        /// <param name="updateEntry">The <see cref="IUpdateEntry"/> to map.</param>
        /// <returns>A new <see cref="WriteModel{TEntity}"/> containing the database changes represented
        /// by <paramref name="updateEntry"/>.</returns>
        WriteModel<TEntity> GenerateWriteModel([NotNull] IUpdateEntry updateEntry);
    }
}