using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Blueshift.EntityFrameworkCore.MongoDB.Metadata;
using Blueshift.EntityFrameworkCore.MongoDB.Update;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Driver;
using Remotion.Linq;

namespace Blueshift.EntityFrameworkCore.MongoDB.Storage
{
    /// <summary>
    ///     The main interaction point between a context and MongoDB.
    ///     This type is typically used by database providers (and other extensions). It
    ///     is generally not used in application code.
    /// </summary>
    public class MongoDbDatabase : Database
    {
        private static readonly MethodInfo GenericSaveChanges = typeof(MongoDbDatabase).GetTypeInfo()
            .GetMethod(nameof(SaveChanges), BindingFlags.NonPublic | BindingFlags.Instance)
            .GetGenericMethodDefinition();

        private static readonly MethodInfo GenericSaveChangesAsync = typeof(MongoDbDatabase).GetTypeInfo()
            .GetMethod(nameof(SaveChangesAsync), BindingFlags.NonPublic | BindingFlags.Instance)
            .GetGenericMethodDefinition();

        private readonly IMongoDbConnection _mongoDbConnection;
        private readonly IMongoDbWriteModelFactorySelector _mongoDbWriteModelFactorySelector;

        /// <summary>
        /// Initializes a new instance of hte <see cref="MongoDbDatabase"/> class.
        /// </summary>
        /// <param name="databaseDependencies">Parameter object containing dependencies for this service.</param>
        /// <param name="mongoDbConnection">A <see cref="IMongoDbConnection"/> used to communicate with the MongoDB instance.</param>
        /// <param name="mongoDbWriteModelFactorySelector">The <see cref="IMongoDbWriteModelFactorySelector"/> to use to create
        /// <see cref="IMongoDbWriteModelFactory{TEntity}"/> instances.</param>
        public MongoDbDatabase(
            [NotNull] DatabaseDependencies databaseDependencies,
            [NotNull] IMongoDbConnection mongoDbConnection,
            [NotNull] IMongoDbWriteModelFactorySelector mongoDbWriteModelFactorySelector)
            : base(Check.NotNull(databaseDependencies, nameof(databaseDependencies)))
        {
            _mongoDbConnection = Check.NotNull(mongoDbConnection, nameof(mongoDbConnection));
            _mongoDbWriteModelFactorySelector = Check.NotNull(mongoDbWriteModelFactorySelector, nameof(mongoDbWriteModelFactorySelector));
        }

        /// <summary>
        ///     Persists changes from the supplied entries to the database.
        /// </summary>
        /// <param name="entries">A list of entries to be persisted.</param>
        /// <returns>The number of entries that were persisted.</returns>
        public override int SaveChanges(IReadOnlyList<IUpdateEntry> entries)
            => Check.NotNull(entries, nameof(entries))
                .ToLookup(entry => GetCollectionEntityType(entry.EntityType))
                .Sum(grouping => (int)GenericSaveChanges.MakeGenericMethod(grouping.Key.ClrType)
                    .Invoke(this, new object[] { grouping }));

        private IEntityType GetCollectionEntityType(IEntityType entityType)
        {
            MongoDbEntityTypeAnnotations annotations = entityType.MongoDb();
            while (annotations.IsDerivedType && entityType.BaseType != null)
            {
                entityType = entityType.BaseType;
                annotations = entityType.MongoDb();
            }
            return entityType;
        }

        private int SaveChanges<TEntity>(IEnumerable<IUpdateEntry> entries)
        {
            IEnumerable<WriteModel<TEntity>> writeModels = entries
                .Select(entry => _mongoDbWriteModelFactorySelector.Select<TEntity>(entry).CreateWriteModel(entry))
                .ToList();
            BulkWriteResult result = _mongoDbConnection.GetCollection<TEntity>()
                .BulkWrite(writeModels);
            return (int)(result.DeletedCount + result.InsertedCount + result.ModifiedCount);
        }

        /// <summary>
        ///     Asynchronously persists changes from the supplied entries to the database.
        /// </summary>
        /// <param name="entries">A list of entries to be persisted.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken "/> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A <see cref="Task{TResult}"/> representing the state of the operation. The result contains the number
        ///     of entries that were persisted to the database.
        /// </returns>
        public override async Task<int> SaveChangesAsync(IReadOnlyList<IUpdateEntry> entries,
            CancellationToken cancellationToken = new CancellationToken())
        {
            IEnumerable<Task<int>> tasks = Check.NotNull(entries, nameof(entries))
                .ToLookup(entry => GetCollectionEntityType(entry.EntityType))
                .Select(grouping => InvokeSaveChangesAsync(grouping, cancellationToken));
            return await Task.WhenAll()
                .ContinueWith(allTask => tasks.Sum(task => task.Result), cancellationToken);
        }

        private async Task<int> InvokeSaveChangesAsync(IGrouping<IEntityType, IUpdateEntry> entryGrouping, CancellationToken cancellationToken)
            => await (Task<int>)GenericSaveChangesAsync.MakeGenericMethod(entryGrouping.Key.ClrType)
                .Invoke(this, new object[] {entryGrouping, cancellationToken});

        private async Task<int> SaveChangesAsync<TEntity>(IEnumerable<IUpdateEntry> entries, CancellationToken cancellationToken)
        {
            IEnumerable<WriteModel<TEntity>> writeModels = entries
                .Select(entry => _mongoDbWriteModelFactorySelector.Select<TEntity>(entry).CreateWriteModel(entry))
                .ToList();
            BulkWriteResult result = await _mongoDbConnection.GetCollection<TEntity>()
                .BulkWriteAsync(writeModels, options: null, cancellationToken: cancellationToken);
            return (int)(result.DeletedCount + result.InsertedCount + result.ModifiedCount);
        }

        /// <inheritdoc />
        public override Func<QueryContext, IAsyncEnumerable<TResult>> CompileAsyncQuery<TResult>(QueryModel queryModel)
            => queryContext => CompileQuery<TResult>(queryModel)(queryContext).ToAsyncEnumerable();
    }
}