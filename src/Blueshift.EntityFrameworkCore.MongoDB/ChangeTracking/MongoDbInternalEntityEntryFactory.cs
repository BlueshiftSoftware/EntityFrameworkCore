using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace Blueshift.EntityFrameworkCore.MongoDB.ChangeTracking
{
    /// <inheritdoc />
    public class MongoDbInternalEntityEntryFactory : InternalEntityEntryFactory
    {
        /// <inheritdoc />
        public override InternalEntityEntry Create(
            IStateManager stateManager,
            IEntityType entityType,
            object entity,
            ValueBuffer valueBuffer)
            =>  base.Create(stateManager, entityType, entity);
    }
}
