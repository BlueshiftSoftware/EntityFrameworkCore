using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query
{
    /// <summary>
    /// Interface for a service that can create instances of <see cref="EntityLoadInfo"/>.
    /// </summary>
    public interface IEntityLoadInfoFactory
    {
        /// <summary>
        /// Creates a new instance of <see cref="EntityLoadInfo"/> for the given <paramref name="document"/> instance.
        /// </summary>
        /// <param name="document">The object for which the <see cref="EntityLoadInfo"/> will be created.</param>
        /// <param name="entityType">The <see cref="IEntityType"/> representing the type of <paramref name="document"/>.</param>
        /// <param name="owner">The entity instance that owns <paramref name="document"/>, if any.</param>
        /// <param name="owningNavigation">The <see cref="INavigation"/> that describes the ownership, if any.</param>
        /// <returns>A new instance of <see cref="EntityLoadInfo"/> that can be used to load the given <paramref name="document"/>.</returns>
        EntityLoadInfo Create(
            [NotNull] object document,
            [NotNull] IEntityType entityType,
            [CanBeNull] object owner,
            [CanBeNull] INavigation owningNavigation);
    }
}
