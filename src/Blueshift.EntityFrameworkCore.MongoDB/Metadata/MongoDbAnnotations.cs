using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.MongoDB.Metadata
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class MongoDbAnnotations<TAnnotatable>
        where TAnnotatable : IAnnotatable
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected MongoDbAnnotations([NotNull] TAnnotatable metadata)
        {
            Annotatable = Check.Is<IMutableAnnotatable>(metadata, nameof(metadata));
            Metadata = Check.NotNull(metadata, nameof(metadata));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual TAnnotatable Metadata { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IMutableAnnotatable Annotatable { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual T GetAnnotation<T>([CanBeNull] string annotationName)
            => (T)Annotatable[Check.NullButNotEmpty(annotationName, nameof(annotationName))];

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool SetAnnotation<T>([NotNull] string annotationName, [CanBeNull] T value)
        {
            Check.NotEmpty(annotationName, nameof(annotationName));
            Annotatable[annotationName] = value;
            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool CanSetAnnotation([NotNull] string annotationName, [CanBeNull] object value)
            => true;
    }
}