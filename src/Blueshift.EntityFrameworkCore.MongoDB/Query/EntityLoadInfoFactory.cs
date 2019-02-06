using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query
{
    /// <inheritdoc />
    public class EntityLoadInfoFactory : IEntityLoadInfoFactory
    {
        [NotNull] private readonly ICurrentDbContext _currentDbContext;
        [NotNull] private readonly IValueBufferFactory _valueBufferFactory;

        /// <summary>
        /// Initializes a new instance of <see cref="EntityLoadInfoFactory"/>.
        /// </summary>
        /// <param name="currentDbContext">Used to get the current <see cref="DbContext"/> instance.</param>
        /// <param name="valueBufferFactory">An <see cref="IValueBufferFactory"/> that can be used to create <see cref="ValueBuffer"/>
        /// for loading documents.</param>
        public EntityLoadInfoFactory(
            [NotNull] ICurrentDbContext currentDbContext,
            [NotNull] IValueBufferFactory valueBufferFactory)
        {
            _currentDbContext = Check.NotNull(currentDbContext, nameof(currentDbContext));
            _valueBufferFactory = Check.NotNull(valueBufferFactory, nameof(valueBufferFactory));
        }

        /// <inheritdoc />
        public EntityLoadInfo Create(object document, IEntityType entityType, object owner, INavigation owningNavigation)
        {
            Check.NotNull(document, nameof(document));
            Check.NotNull(entityType, nameof(entityType));

            if (document.GetType() != entityType.ClrType)
            {
                Check.IsInstanceOfType(document, entityType.ClrType, nameof(document));

                entityType = entityType.Model.FindEntityType(document.GetType());
            }

            return new EntityLoadInfo(
                new MaterializationContext(
                    _valueBufferFactory.CreateFromInstance(
                        document,
                        entityType,
                        owner,
                        owningNavigation),
                    _currentDbContext.Context),
                materializationContext => document,
                null);
        }
    }
}