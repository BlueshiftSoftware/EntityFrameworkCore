using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blueshift.EntityFrameworkCore.MongoDB.Metadata;
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

        /// <summary>
        ///     Gets the <see cref="IMongoDatabase"/> used by the current model.
        /// </summary>
        /// <returns>The <see cref="IMongoDatabase"/> used by the MongoDB C# driver to communicate with the MongoDB instance.</returns>
        public virtual IMongoDatabase GetDatabase()
            => _mongoDatabase;

        /// <summary>
        ///     Asynchronously gets the <see cref="IMongoDatabase"/> used by the current model.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken "/> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A <see cref="Task{TResult}"/> representing the state of the operation. The result contains The
        ///     <see cref="IMongoDatabase"/> used by the MongoDB C# driver to communicate with the MongoDB instance.
        /// </returns>
        public virtual async Task<IMongoDatabase> GetDatabaseAsync(CancellationToken cancellationToken = default(CancellationToken))
            => await Task.FromResult(_mongoDatabase);

        /// <summary>
        ///     Drops the database used by this model from the MongoDB instance.
        /// </summary>
        public virtual void DropDatabase()
            => _mongoClient.DropDatabase(new MongoDbModelAnnotations(_model).Database);

        /// <summary>
        ///     Asynchronously drops the database used by this model from the MongoDB instance.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken "/> to observe while waiting for the task to complete.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the state of the operation.</returns>
        public virtual async Task DropDatabaseAsync(CancellationToken cancellationToken = default(CancellationToken))
            => await _mongoClient.DropDatabaseAsync(new MongoDbModelAnnotations(_model).Database, cancellationToken);

        /// <summary>
        ///     Gets a <see cref="IMongoCollection{TEntity}"/> instance that can be used to store instances of <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity stored in the collection.</typeparam>
        /// <returns>The <see cref="IMongoCollection{TEntity}"/> instance that can store <typeparamref name="TEntity"/>.</returns>
        public virtual IMongoCollection<TEntity> GetCollection<TEntity>()
        {
            IEntityType entityType = _model.FindEntityType(typeof(TEntity));
            MongoDbEntityTypeAnnotations annotations = entityType.MongoDb();
            while (annotations.IsDerivedType && entityType.BaseType != null)
            {
                entityType = entityType.BaseType;
                annotations = entityType.MongoDb();
            }
            return _mongoDatabase.GetCollection<TEntity>(annotations.CollectionName, annotations.CollectionSettings);
        }

        /// <summary>
        ///     Gets a <see cref="IQueryable{TEntity}"/> that can be used to query instances of <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity to query.</typeparam>
        /// <returns>An <see cref="IQueryable{TEntity}"/> that can query <typeparamref name="TEntity"/> values from the MongoDB instance.</returns>
        public virtual IQueryable<TEntity> Query<TEntity>()
            => GetCollection<TEntity>().AsQueryable();
    }
}