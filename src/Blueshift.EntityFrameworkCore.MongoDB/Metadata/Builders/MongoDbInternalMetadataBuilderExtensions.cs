using Blueshift.EntityFrameworkCore.MongoDB.Metadata;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class MongoDbInternalMetadataBuilderExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static MongoDbEntityTypeAnnotations MongoDb([NotNull] this InternalEntityTypeBuilder internalEntityTypeBuilder)
            => MongoDb(Check.NotNull(internalEntityTypeBuilder, nameof(internalEntityTypeBuilder)).Metadata);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static MongoDbEntityTypeAnnotations MongoDb([NotNull] this EntityTypeBuilder entityTypeBuilder)
            => MongoDb(Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder)).Metadata);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static MongoDbEntityTypeAnnotations MongoDb([NotNull] this IEntityType entityType)
            => MongoDb(Check.Is<IMutableEntityType>(entityType, nameof(entityType)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static MongoDbEntityTypeAnnotations MongoDb([NotNull] this IMutableEntityType mutableEntityType)
            => new MongoDbEntityTypeAnnotations(Check.NotNull(mutableEntityType, nameof(mutableEntityType)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static MongoDbNavigationAnnotations MongoDb([NotNull] this INavigation navigation)
            => new MongoDbNavigationAnnotations(Check.NotNull(navigation, nameof(navigation)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static MongoDbModelAnnotations MongoDb([NotNull] this InternalModelBuilder internalModelBuilder)
            => MongoDb(Check.NotNull(internalModelBuilder, nameof(internalModelBuilder)).Metadata);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static MongoDbModelAnnotations MongoDb([NotNull] this ModelBuilder modelBuilder)
            => MongoDb(Check.NotNull(modelBuilder, nameof(modelBuilder)).Model);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static MongoDbModelAnnotations MongoDb([NotNull] this IModel model)
            => MongoDb(Check.Is<IMutableModel>(model, nameof(model)));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static MongoDbModelAnnotations MongoDb([NotNull] this IMutableModel mutableModel)
            => new MongoDbModelAnnotations(Check.NotNull(mutableModel, nameof(mutableModel)));
    }
}