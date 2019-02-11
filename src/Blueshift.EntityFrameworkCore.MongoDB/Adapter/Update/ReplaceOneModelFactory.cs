using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using MongoDB.Driver;

namespace Blueshift.EntityFrameworkCore.MongoDB.Adapter.Update
{
    /// <inheritdoc />
    /// <summary>
    /// Creates <see cref="UpdateOneModel{TEntity}"/> from a given <see cref="IUpdateEntry"/>.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity being added.</typeparam>
    public class ReplaceOneModelFactory<TEntity> : MongoDbWriteModelFactory<TEntity>
    {
        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="ReplaceOneModelFactory{TEntity}" /> class.
        /// </summary>
        /// <param name="valueGeneratorSelector">The <see cref="IValueGeneratorSelector" /> to use for populating
        /// concurrency tokens.</param>
        /// <param name="entityType">The <see cref="IEntityType" /> for which this
        /// <see cref="ReplaceOneModelFactory{TEntity}" /> will be used.</param>
        public ReplaceOneModelFactory(
            [NotNull] IValueGeneratorSelector valueGeneratorSelector,
            [NotNull] IEntityType entityType)
            : base(
                Check.NotNull(valueGeneratorSelector, nameof(valueGeneratorSelector)),
                Check.NotNull(entityType, nameof(entityType)))
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Creates an <see cref="UpdateOneModel{TEntity}"/> that maps the given <paramref name="updateEntry"/>.
        /// </summary>
        /// <param name="updateEntry">The <see cref="IUpdateEntry"/> to map.</param>
        /// <returns>A new <see cref="UpdateOneModel{TEntity}"/> containing the inserted values represented
        /// by <paramref name="updateEntry"/>.</returns>
        public override WriteModel<TEntity> CreateWriteModel(IUpdateEntry updateEntry)
        {
            InternalEntityEntry internalEntityEntry = Check.Is<InternalEntityEntry>(updateEntry, nameof(updateEntry));
            FilterDefinition<TEntity> lookupFilter = GetLookupFilter(updateEntry);
            UpdateConcurrencyProperties(internalEntityEntry);

            return new ReplaceOneModel<TEntity>(
                lookupFilter,
                (TEntity)internalEntityEntry.Entity);
        }
    }
}