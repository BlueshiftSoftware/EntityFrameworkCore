using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Blueshift.EntityFrameworkCore.MongoDB.Storage
{
    /// <summary>
    ///     An interface for a service that can be used to interact with a MongoDB instance.
    /// </summary>
    public interface IMongoDbConnection
    {
        /// <summary>
        ///     Gets the <see cref="IMongoDatabase"/> used by the current model.
        /// </summary>
        /// <returns>The <see cref="IMongoDatabase"/> used by the MongoDB C# driver to communicate with the MongoDB instance.</returns>
        IMongoDatabase GetDatabase();

        /// <summary>
        ///     Asynchronously gets the <see cref="IMongoDatabase"/> used by the current model.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken "/> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A <see cref="Task{TResult}"/> representing the state of the operation. The result contains The
        ///     <see cref="IMongoDatabase"/> used by the MongoDB C# driver to communicate with the MongoDB instance.
        /// </returns>
        Task<IMongoDatabase> GetDatabaseAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///     Drops the database used by this model from the MongoDB instance.
        /// </summary>
        void DropDatabase();

        /// <summary>
        ///     Asynchronously drops the database used by this model from the MongoDB instance.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken "/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the state of the operation.</returns>
        Task DropDatabaseAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///     Gets a <see cref="IMongoCollection{TEntity}"/> instance that can be used to store instances of <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity stored in the collection.</typeparam>
        /// <returns>The <see cref="IMongoCollection{TEntity}"/> instance that can store <typeparamref name="TEntity"/>.</returns>
        IMongoCollection<TEntity> GetCollection<TEntity>();

        /// <summary>
        ///     Gets a <see cref="IQueryable{TEntity}"/> that can be used to query instances of <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity to query.</typeparam>
        /// <returns>An <see cref="IQueryable{TEntity}"/> that can query <typeparamref name="TEntity"/> values from the MongoDB instance.</returns>
        IQueryable<TEntity> Query<TEntity>();
    }
}