using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.MongoDB.Metadata.Builders
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class DocumentInternalKeyBuilderExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static InternalKeyBuilder IsDocumentOwnershipKey(
            [NotNull] this InternalKeyBuilder internalKeyBuilder,
            bool isDocumentOwnershipKey)
        {
            DocumentKeyAnnotations documentKeyAnnotations =
                Check.NotNull(internalKeyBuilder, nameof(internalKeyBuilder)).Document();

            documentKeyAnnotations.IsOwnershipKey = isDocumentOwnershipKey;

            return internalKeyBuilder;
        }
            
    }
}
