using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.MongoDB.Metadata
{
    /// <inheritdoc />
    public class MongoDbNavigationAnnotations : MongoDbAnnotations<INavigation>
    {
        /// <inheritdoc />
        public MongoDbNavigationAnnotations([NotNull] INavigation navigation)
            : base(navigation)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string NavigationName
        {
            get { return GetAnnotation<string>(MongoDbAnnotationNames.NavigationName); }
            [param: NotNull]
            set { SetAnnotation(MongoDbAnnotationNames.NavigationName, Check.NotEmpty(value, nameof(NavigationName))); }
        }
    }
}