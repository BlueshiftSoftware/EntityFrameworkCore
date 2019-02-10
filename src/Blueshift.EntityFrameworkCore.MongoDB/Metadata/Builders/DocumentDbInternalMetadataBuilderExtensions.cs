using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.MongoDB.Metadata.Builders
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class DocumentInternalMetadataBuilderExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static DocumentEntityTypeAnnotations Document([NotNull] this InternalEntityTypeBuilder internalEntityTypeBuilder)
            => Check.NotNull(internalEntityTypeBuilder, nameof(internalEntityTypeBuilder)).Metadata.Document();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static DocumentEntityTypeAnnotations Document([NotNull] this EntityTypeBuilder entityTypeBuilder)
            => Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder)).Metadata.Document();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static DocumentEntityTypeAnnotations Document([NotNull] this EntityType entityType)
            => Check.NotNull<IEntityType>(entityType, nameof(entityType)).Document();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static DocumentEntityTypeAnnotations Document([NotNull] this IEntityType entityType)
            => new DocumentEntityTypeAnnotations(Check.NotNull(entityType, nameof(entityType)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static DocumentKeyAnnotations Document([NotNull] this InternalKeyBuilder internalKeyBuilder)
            => Check.NotNull(internalKeyBuilder, nameof(internalKeyBuilder)).Metadata.Document();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static DocumentKeyAnnotations Document([NotNull] this Key key)
            => Check.NotNull<IKey>(key, nameof(key)).Document();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static DocumentKeyAnnotations Document([NotNull] this IKey key)
            => new DocumentKeyAnnotations(Check.NotNull(key, nameof(key)));
    }
}