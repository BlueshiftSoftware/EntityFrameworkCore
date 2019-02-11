using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using MongoDB.Driver;

namespace Blueshift.EntityFrameworkCore.MongoDB.Adapter.Update
{
    /// <summary>
    /// Creates <see cref="DeleteOneModel{TEntity}"/> from a given <see cref="IUpdateEntry"/>.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity being added.</typeparam>
    public class DeleteOneModelFactory<TEntity> : MongoDbWriteModelFactory<TEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDbWriteModelFactory{TEntity}"/> class.
        /// </summary>
        /// <param name="valueGeneratorSelector">The <see cref="IValueGeneratorSelector"/> to use for populating concurrency tokens.</param>
        /// <param name="entityType">The <see cref="IEntityType"/> for which this <see cref="MongoDbWriteModelFactory{TDocument}"/> will be used.</param>
        public DeleteOneModelFactory(
            [NotNull] IValueGeneratorSelector valueGeneratorSelector,
            [NotNull] IEntityType entityType)
            : base(
                Check.NotNull(valueGeneratorSelector, nameof(valueGeneratorSelector)),
                Check.NotNull(entityType, nameof(entityType)))
        {
        }

        /// <summary>
        /// Creates an <see cref="DeleteOneModel{TEntity}"/> that maps the given <paramref name="updateEntry"/>.
        /// </summary>
        /// <param name="updateEntry">The <see cref="IUpdateEntry"/> to map.</param>
        /// <returns>A new <see cref="DeleteOneModel{TEntity}"/> containing the inserted values represented
        /// by <paramref name="updateEntry"/>.</returns>
        public override WriteModel<TEntity> CreateWriteModel(IUpdateEntry updateEntry)
            => new DeleteOneModel<TEntity>(GetLookupFilter(updateEntry));
    }
}