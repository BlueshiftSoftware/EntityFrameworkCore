using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.MongoDB.Storage
{
    /// <summary>
    ///     Creates and deletes databases for a given database provider.
    ///     This interface is typically used by database providers (and other extensions).
    ///     It is generally not used in application code.
    /// </summary>
    public class MongoDbDatabaseCreator : IDatabaseCreator
    {
        private readonly IMongoDbConnection _mongoDbConnection;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MongoDbDatabaseCreator"/> class.
        /// </summary>
        /// <param name="mongoDbConnection">The <see cref="IMongoDbConnection"/> used to communicate with the MongoDB instance.</param>
        public MongoDbDatabaseCreator([NotNull] IMongoDbConnection mongoDbConnection)
        {
            _mongoDbConnection = Check.NotNull(mongoDbConnection, nameof(mongoDbConnection));
        }

        /// <summary>
        ///     Ensures that the database for the context exists.
        /// </summary>
        /// <returns>
        ///     MongoDB databases will always be created when they are first referenced, so this method will always
        ///     return <code>false</code>.
        /// </returns>
        public virtual bool EnsureCreated()
        {
            _mongoDbConnection.GetDatabase();
            return false;
        }

        /// <summary>
        ///     Asynchronously ensures that the database for the context exists.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous save operation. MongoDB databases will always be created when they are
        ///     first referenced, so the result will always contain <code>false</code>.
        /// </returns>
        public virtual async Task<bool> EnsureCreatedAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            await _mongoDbConnection.GetDatabaseAsync(cancellationToken);
            return false;
        }

        /// <summary>
        ///     Ensures that the database for the context does not exist.
        /// </summary>
        /// <returns>
        ///     MongoDB database are always created when they are first referenced, so this method
        ///     will always return <code>true</code>.
        /// </returns>
        public virtual bool EnsureDeleted()
        {
            _mongoDbConnection.DropDatabase();
            return true;
        }

        /// <summary>
        ///     Asynchronously ensures that the database for the context does not exist.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken "/> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous save operation. MongoDB database are always created when they are first
        ///     referenced, so the result will always contain <code>true</code>.
        /// </returns>
        public virtual async Task<bool> EnsureDeletedAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            await _mongoDbConnection.DropDatabaseAsync(cancellationToken);
            return true;
        }
    }
}