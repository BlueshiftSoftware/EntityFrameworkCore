using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Driver;

namespace Blueshift.EntityFrameworkCore.MongoDB.Metadata
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class DocumentEntityTypeAnnotations : DocumentAnnotations<IEntityType>
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public DocumentEntityTypeAnnotations([NotNull] IEntityType entityType)
            : base(entityType)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsComplexType
        {
            get => GetAnnotation<bool?>(DocumentAnnotationNames.IsComplexType) ?? false;
            set => SetAnnotation(DocumentAnnotationNames.IsComplexType, value);
        }
    }
}