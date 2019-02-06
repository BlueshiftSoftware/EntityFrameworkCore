using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace Blueshift.EntityFrameworkCore.MongoDB.Query
{
    /// <summary>
    /// Interface for a service that can create <see cref="ValueBuffer"/> instances.
    /// </summary>
    public interface IValueBufferFactory
    {
        /// <summary>
        /// Creates an instance of <see cref="ValueBuffer"/> from the given <paramref name="instance"/>.
        /// </summary>
        /// <param name="instance">An existing object instance to use to build the value buffer.</param>
        /// <param name="entityType">The <see cref="IEntityType"/> containing the metadata for <paramref name="instance"/>.</param>
        /// <param name="owner">The entity that owns <paramref name="instance"/>, if any.</param>
        /// <param name="owningNavigation">The <see cref="INavigation"/> that describes the ownership, if any.</param>
        /// <returns>A new <see cref="ValueBuffer"/> that reflects the properties of the given <paramref name="instance"/>.</returns>
        ValueBuffer CreateFromInstance(
            [NotNull] object instance,
            [NotNull] IEntityType entityType,
            [CanBeNull] object owner,
            [CanBeNull] INavigation owningNavigation);
    }
}
