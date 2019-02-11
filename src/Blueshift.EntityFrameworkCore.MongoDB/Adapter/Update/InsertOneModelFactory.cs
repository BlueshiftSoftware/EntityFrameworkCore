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
    /// Creates <see cref="T:MongoDB.Driver.InsertOneModel`1" /> from a given <see cref="T:Microsoft.EntityFrameworkCore.Update.IUpdateEntry" />.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity being added.</typeparam>
    public class InsertOneModelFactory<TEntity> : MongoDbWriteModelFactory<TEntity>
    {
        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Blueshift.EntityFrameworkCore.MongoDB.Adapter.Update.MongoDbWriteModelFactory`1" /> class.
        /// </summary>
        /// <param name="valueGeneratorSelector">The <see cref="T:Microsoft.EntityFrameworkCore.ValueGeneration.IValueGeneratorSelector" /> to use for populating concurrency tokens.</param>
        /// <param name="entityType">The <see cref="T:Microsoft.EntityFrameworkCore.Metadata.IEntityType" /> for which this <see cref="T:Blueshift.EntityFrameworkCore.MongoDB.Adapter.Update.MongoDbWriteModelFactory`1" /> will be used.</param>
        public InsertOneModelFactory(
            [NotNull] IValueGeneratorSelector valueGeneratorSelector,
            [NotNull] IEntityType entityType)
            : base(
                Check.NotNull(valueGeneratorSelector, nameof(valueGeneratorSelector)),
                Check.NotNull(entityType, nameof(entityType)))
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Creates an <see cref="T:MongoDB.Driver.InsertOneModel`1" /> that maps the given <paramref name="updateEntry" />.
        /// </summary>
        /// <param name="updateEntry">The <see cref="T:Microsoft.EntityFrameworkCore.Update.IUpdateEntry" /> to map.</param>
        /// <returns>A new <see cref="T:MongoDB.Driver.InsertOneModel`1" /> containing the inserted values represented
        /// by <paramref name="updateEntry" />.</returns>
        public override WriteModel<TEntity> CreateWriteModel(IUpdateEntry updateEntry)
        {
            InternalEntityEntry internalEntityEntry = Check.Is<InternalEntityEntry>(updateEntry, nameof(updateEntry));
            UpdateConcurrencyProperties(internalEntityEntry);
            return new InsertOneModel<TEntity>((TEntity)internalEntityEntry.Entity);
        }
    }
}