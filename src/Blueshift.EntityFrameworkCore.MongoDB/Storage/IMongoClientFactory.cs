using MongoDB.Driver;

namespace Blueshift.EntityFrameworkCore.MongoDB.Storage
{
    /// <summary>
    /// Interface for a factory service that can create instances of <see cref="IMongoClient"/>.
    /// </summary>
    public interface IMongoClientFactory
    {
        /// <summary>
        /// Creates a new <see cref="IMongoClient"/> that can be used to communicate with the MongoDB instance.
        /// </summary>
        /// <returns>A new <see cref="IMongoClient"/> instance that can communicate with a MongoDB instance.</returns>
        IMongoClient CreateMongoClient();
    }
}
