using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Blueshift.EntityFrameworkCore.MongoDB.Metadata
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class DocumentKeyAnnotations : DocumentAnnotations<IKey>
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public DocumentKeyAnnotations([NotNull] IKey key)
            : base(key)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsOwnershipKey
        {
            get => GetAnnotation<bool?>(DocumentAnnotationNames.IsOwnershipKey) ?? false;
            set => SetAnnotation(DocumentAnnotationNames.IsOwnershipKey, value);
        }
    }
}