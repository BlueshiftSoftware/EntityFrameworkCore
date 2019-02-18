using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Blueshift.EntityFrameworkCore.MongoDB.Adapter.Update;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Driver;

namespace Blueshift.EntityFrameworkCore.MongoDB.Storage
{
    /// <summary>
    ///     The main interaction point between a context and MongoDB.
    ///     This type is typically used by database providers (and other extensions). It
    ///     is generally not used in application code.
    /// </summary>
    public class MongoDbDatabase : Database
    {
        private static readonly MethodInfo GenericUpdateEntries = 
            MethodHelper.GetGenericMethodDefinition(
                (MongoDbDatabase mongoDbDatabase) => mongoDbDatabase.UpdateEntries<object>(null));

        private static readonly MethodInfo GenericUpdateEntriesAsync
            = MethodHelper.GetGenericMethodDefinition(
                (MongoDbDatabase mongoDbDatabase) => mongoDbDatabase.UpdateEntriesAsync<object>(null, CancellationToken.None));

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
            => GetDocumentUpdateDefinitions(entries)
                .ToLookup(entry => entry.EntityType.GetMongoDbCollectionEntityType())
                .Sum(grouping => (int)GenericUpdateEntries.MakeGenericMethod(grouping.Key.ClrType)
                    .Invoke(this, new object[] { grouping }));

        private int UpdateEntries<TEntity>(IEnumerable<IUpdateEntry> entries)
        {
            IEnumerable<WriteModel<TEntity>> writeModels = entries
                .Select(entry => _mongoDbWriteModelFactorySelector.Select<TEntity>(entry).CreateWriteModel(entry))
                .ToList();
            BulkWriteResult result = _mongoDbConnection.GetCollection<TEntity>()
                .BulkWrite(writeModels);
            return (int) (result.DeletedCount + result.InsertedCount + result.ModifiedCount);
        }

        private ISet<IUpdateEntry> GetDocumentUpdateDefinitions(IReadOnlyCollection<IUpdateEntry> entries)
        {
            Check.NotNull(entries, nameof(entries));

            ISet<IUpdateEntry> rootEntries = new HashSet<IUpdateEntry>();

            foreach (IUpdateEntry updateEntry in entries)
            {
                if (updateEntry.EntityType.IsDocumentRootEntityType())
                {
                    rootEntries.Add(updateEntry);
                }
                else if (updateEntry is InternalEntityEntry internalEntityEntry)
                {
                    rootEntries.Add(GetRootDocument(internalEntityEntry));
                }
                else
                {
                    // TBD - throw error
                }
            }

            return rootEntries;
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
        public override async Task<int> SaveChangesAsync(
            IReadOnlyList<IUpdateEntry> entries,
            CancellationToken cancellationToken = default)
        {
            IEnumerable<Task<int>> tasks = GetDocumentUpdateDefinitions(entries)
                .ToLookup(entry => entry.EntityType.GetMongoDbCollectionEntityType())
                .Select(grouping => InvokeUpdateEntriesAsync(grouping, cancellationToken))
                .ToList();

            int[] totals = await Task.WhenAll(tasks);
            return totals.Sum();
        }

        private Task<int> InvokeUpdateEntriesAsync(IGrouping<IEntityType, IUpdateEntry> entryGrouping, CancellationToken cancellationToken)
            => (Task<int>)GenericUpdateEntriesAsync.MakeGenericMethod(entryGrouping.Key.ClrType)
                .Invoke(this, new object[] {entryGrouping, cancellationToken});

        private async Task<int> UpdateEntriesAsync<TEntity>(
            IEnumerable<IUpdateEntry> entries,
            CancellationToken cancellationToken)
        {
            IEnumerable<WriteModel<TEntity>> writeModels = entries
                .Select(entry => _mongoDbWriteModelFactorySelector.Select<TEntity>(entry).CreateWriteModel(entry))
                .ToList();
            BulkWriteResult result = await _mongoDbConnection.GetCollection<TEntity>()
                .BulkWriteAsync(writeModels,
                    options: null,
                    cancellationToken: cancellationToken);
            return (int) (result.DeletedCount + result.InsertedCount + result.ModifiedCount);
        }

        private IUpdateEntry GetRootDocument(InternalEntityEntry entry)
        {
            var stateManager = entry.StateManager;

            InternalEntityEntry owningEntityEntry = entry.EntityType
                .GetForeignKeys()
                .Where(foreignKey => foreignKey.IsOwnership)
                .Select(foreignKey => stateManager.GetPrincipal(entry, foreignKey))
                .SingleOrDefault(owner => owner != null);

            if (owningEntityEntry == null)
            {
                throw new InvalidOperationException($"Encountered orphaned document of type {entry.EntityType.DisplayName()}.");
            }

            return owningEntityEntry.EntityType.IsDocumentRootEntityType()
                ? owningEntityEntry
                : GetRootDocument(owningEntityEntry);
        }
    }
}