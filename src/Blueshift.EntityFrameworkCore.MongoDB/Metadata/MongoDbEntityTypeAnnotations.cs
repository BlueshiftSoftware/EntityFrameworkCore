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
    public class MongoDbEntityTypeAnnotations : MongoDbAnnotations<IEntityType>
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public MongoDbEntityTypeAnnotations([NotNull] IEntityType entityType)
            : base(entityType)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool AssignIdOnInsert
        {
            get => CollectionSettings?.AssignIdOnInsert ?? false;
            set => GetOrCreateCollectionSettings().AssignIdOnInsert = value;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string CollectionName
        {
            get => GetAnnotation<string>(MongoDbAnnotationNames.CollectionName)
                   ?? MongoDbUtilities.Pluralize(MongoDbUtilities.ToLowerCamelCase(Metadata.ClrType.Name));
            [param: NotNull]
            set => SetAnnotation(MongoDbAnnotationNames.CollectionName, Check.NotEmpty(value, nameof(CollectionName)));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MongoCollectionSettings CollectionSettings
        {
            get => GetAnnotation<MongoCollectionSettings>(MongoDbAnnotationNames.CollectionSettings);
            [param: NotNull]
            set => SetAnnotation(MongoDbAnnotationNames.CollectionSettings, Check.NotNull(value, nameof(CollectionSettings)));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Discriminator
        {
            get => GetAnnotation<string>(MongoDbAnnotationNames.Discriminator) ?? Metadata.ClrType.Name;
            [param: NotNull]
            set => SetAnnotation(MongoDbAnnotationNames.Discriminator, Check.NotEmpty(value, nameof(Discriminator)));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool DiscriminatorIsRequired
        {
            get => GetAnnotation<bool?>(MongoDbAnnotationNames.DiscriminatorIsRequired) ?? false;
            set => SetAnnotation(MongoDbAnnotationNames.DiscriminatorIsRequired, value);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsComplexType
        {
            get => GetAnnotation<bool?>(MongoDbAnnotationNames.IsComplexType) ?? false;
            set => SetAnnotation(MongoDbAnnotationNames.IsComplexType, value);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsDerivedType
        {
            get => GetAnnotation<bool?>(MongoDbAnnotationNames.IsDerivedType) ?? false;
            set => SetAnnotation(MongoDbAnnotationNames.IsDerivedType, value);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsRootType
        {
            get => GetAnnotation<bool?>(MongoDbAnnotationNames.IsRootType) ?? false;
            set => SetAnnotation(MongoDbAnnotationNames.IsRootType, value);
        }

        private MongoCollectionSettings GetOrCreateCollectionSettings()
            => CollectionSettings ?? (CollectionSettings = new MongoCollectionSettings());
    }
}