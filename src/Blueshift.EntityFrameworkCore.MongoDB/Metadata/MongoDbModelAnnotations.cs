using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Driver;

namespace Blueshift.EntityFrameworkCore.MongoDB.Metadata
{
    /// <inheritdoc />
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class MongoDbModelAnnotations : MongoDbAnnotations<IModel>
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public MongoDbModelAnnotations([NotNull] IModel model)
            : base(model)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Database
        {
            get { return GetAnnotation<string>(MongoDbAnnotationNames.Database); }
            [param: NotNull]
            set { SetAnnotation(MongoDbAnnotationNames.Database, Check.NotEmpty(value, nameof(Database))); }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MongoDatabaseSettings DatabaseSettings
        {
            get { return GetAnnotation<MongoDatabaseSettings>(MongoDbAnnotationNames.DatabaseSettings); }
            [param: NotNull]
            set { SetAnnotation(MongoDbAnnotationNames.DatabaseSettings, Check.NotNull(value, nameof(DatabaseSettings))); }
        }
    }
}