using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.MongoDB.ChangeTracking
{
    /// <inheritdoc />
    public class MongoDbStateManager : StateManager
    {
        /// <inheritdoc />
        public MongoDbStateManager([NotNull] StateManagerDependencies dependencies)
            : base(Check.NotNull(dependencies, nameof(dependencies)))
        {
        }

        /// <inheritdoc />
        public override IReadOnlyList<InternalEntityEntry> GetInternalEntriesToSave()
            => base
                .GetInternalEntriesToSave()
                .Where(internalEntityEntry => !internalEntityEntry.EntityType.MongoDb().IsComplexType)
                .ToList();
    }
}
