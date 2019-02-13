using System;
using System.Threading;
using System.Threading.Tasks;
using Blueshift.EntityFrameworkCore.MongoDB.Metadata;
using Blueshift.EntityFrameworkCore.MongoDB.Metadata.Builders;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Blueshift.EntityFrameworkCore.MongoDB.Storage
{
    /// <inheritdoc />
    /// <summary>
    ///     A service that can be used to interact with a MongoDB instance.
    /// </summary>
    public class MongoDbConnection : IMongoDbConnection
    {
        private readonly MongoDbModelAnnotations _mongoDbModelAnnotations;
        private readonly Lazy<IMongoClient> _lazyMongoClient;
        private readonly IModel _model;

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDbConnection"/> class.
        /// </summary>
        /// <param name="mongoClientFactory">The <see cref="IMongoClientFactory"/> used to create the underlying
        /// <see cref="IMongoClient"/> used to communicate with the MongoDB server.</param>
        /// <param name="model">The <see cref="IModel"/> used by this connection.</param>
        public MongoDbConnection(
            [NotNull] IMongoClientFactory mongoClientFactory,
            [NotNull] IModel model)
        {
            _model = Check.NotNull(model, nameof(model));

            _mongoDbModelAnnotations = model.MongoDb();

            _lazyMongoClient = new Lazy<IMongoClient>(mongoClientFactory.CreateMongoClient);
        }

        /// <inheritdoc />
        public virtual IMongoClient MongoClient
            => _lazyMongoClient.Value;

        /// <inheritdoc />
        public virtual IMongoDatabase GetDatabase()
            => MongoClient.GetDatabase(
                _mongoDbModelAnnotations.Database,
                _mongoDbModelAnnotations.DatabaseSettings);

        /// <inheritdoc />
        public virtual Task<IMongoDatabase> GetDatabaseAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(GetDatabase());
        }

        /// <inheritdoc />
        public virtual void DropDatabase()
            => MongoClient.DropDatabase(_mongoDbModelAnnotations.Database);

        /// <inheritdoc />
        public virtual async Task DropDatabaseAsync(CancellationToken cancellationToken = default)
            => await MongoClient
                .DropDatabaseAsync(_model.MongoDb().Database, cancellationToken)
                .ConfigureAwait(false);

        /// <inheritdoc />
        public virtual IMongoCollection<TEntity> GetCollection<TEntity>()
        {
            IEntityType collectionEntityType = _model
                .FindEntityType(typeof(TEntity))
                .GetMongoDbCollectionEntityType();

            MongoDbEntityTypeAnnotations annotations = collectionEntityType.MongoDb();

            return GetDatabase()
                .GetCollection<TEntity>(annotations.CollectionName, annotations.CollectionSettings);
        }

        /// <inheritdoc />
        public virtual IMongoQueryable<TEntity> Query<TEntity>()
            => GetCollection<TEntity>().AsQueryable();
    }
}