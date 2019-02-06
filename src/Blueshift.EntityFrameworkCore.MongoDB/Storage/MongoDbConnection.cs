using System.Threading;
using System.Threading.Tasks;
using Blueshift.EntityFrameworkCore.MongoDB.Metadata;
using Blueshift.EntityFrameworkCore.MongoDB.Metadata.Builders;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Driver;

namespace Blueshift.EntityFrameworkCore.MongoDB.Storage
{
    /// <summary>
    ///     A service that can be used to interact with a MongoDB instance.
    /// </summary>
    public class MongoDbConnection : IMongoDbConnection
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoDatabase _mongoDatabase;
        private readonly IModel _model;

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDbConnection"/> class.
        /// </summary>
        /// <param name="mongoClient">The <see cref="IMongoClient"/> used to communicate with the MongoDB instance.</param>
        /// <param name="model">The <see cref="IModel"/> used by this connection.</param>
        public MongoDbConnection(
            [NotNull] IMongoClient mongoClient,
            [NotNull] IModel model)
        {
            _model = Check.NotNull(model, nameof(model));

            _mongoClient = Check.NotNull(mongoClient, nameof(mongoClient));
            _mongoDatabase = _mongoClient.GetDatabase(new MongoDbModelAnnotations(model).Database);
        }

        /// <inheritdoc />
        public virtual IMongoDatabase GetDatabase()
            => _mongoDatabase;

        /// <inheritdoc />
        public virtual Task<IMongoDatabase> GetDatabaseAsync(CancellationToken cancellationToken = default(CancellationToken))
            => Task.FromResult(_mongoDatabase);

        /// <inheritdoc />
        public virtual void DropDatabase()
            => _mongoClient.DropDatabase(new MongoDbModelAnnotations(_model).Database);

        /// <inheritdoc />
        public virtual Task DropDatabaseAsync(CancellationToken cancellationToken = default(CancellationToken))
            => _mongoClient.DropDatabaseAsync(new MongoDbModelAnnotations(_model).Database, cancellationToken);

        /// <inheritdoc />
        public virtual IMongoCollection<TEntity> GetCollection<TEntity>()
        {
            IEntityType collectionEntityType = _model
                .FindEntityType(typeof(TEntity))
                .GetMongoDbCollectionEntityType();

            MongoDbEntityTypeAnnotations annotations = collectionEntityType.MongoDb();

            return _mongoDatabase.GetCollection<TEntity>(annotations.CollectionName, annotations.CollectionSettings);
        }
    }
}