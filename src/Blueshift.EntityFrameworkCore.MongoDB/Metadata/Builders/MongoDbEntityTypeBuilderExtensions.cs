using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.MongoDB.Metadata.Builders
{
    /// <summary>
    ///     Provides a set of MongoDB-specific extension methods for <see cref="EntityTypeBuilder"/>.
    /// </summary>
    public static class MongoDbEntityTypeBuilderExtensions
    {
        /// <summary>
        ///     Sets the name of the MongoDB collection used to store the <see cref="IEntityType"/> being built.
        /// </summary>
        /// <param name="entityTypeBuilder">The <see cref="EntityTypeBuilder"/> to annotate.</param>
        /// <param name="collectionName">The name of the MongoDB collection.</param>
        /// <returns>The <paramref name="entityTypeBuilder"/>, such that calls be chained.</returns>
        public static EntityTypeBuilder ForMongoDbFromCollection(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            [NotNull] string collectionName)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NotEmpty(collectionName, nameof(collectionName));

            entityTypeBuilder.MongoDb().CollectionName = collectionName;
            return entityTypeBuilder;
        }

        /// <summary>
        ///     Sets the discriminator used to query instances of the <see cref="IEntityType"/> being built.
        /// </summary>
        /// <param name="entityTypeBuilder">The <see cref="EntityTypeBuilder"/> to annotate.</param>
        /// <param name="discriminator">The discriminator for the <see cref="IEntityType"/>.</param>
        /// <returns>The <paramref name="entityTypeBuilder"/>, such that calls be chained.</returns>
        public static EntityTypeBuilder ForMongoDbHasDiscriminator(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            [NotNull] string discriminator)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NotEmpty(discriminator, nameof(discriminator));

            entityTypeBuilder.MongoDb().Discriminator = discriminator;
            return entityTypeBuilder;
        }

        /// <summary>
        ///     Sets the whether or not a discriminator is required to query instances of the <see cref="IEntityType"/> being built.
        /// </summary>
        /// <param name="entityTypeBuilder">The <see cref="EntityTypeBuilder"/> to annotate.</param>
        /// <param name="discriminatorIsRequired">
        ///     <code>true</code> if a discriminator is required to query instances of the entity; otherwise <code>false</code>.
        /// </param>
        /// <returns>The <paramref name="entityTypeBuilder"/>, such that calls be chained.</returns>
        public static EntityTypeBuilder ForMongoDbDiscriminatorIsRequired(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            bool discriminatorIsRequired)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            entityTypeBuilder.MongoDb().DiscriminatorIsRequired = discriminatorIsRequired;
            return entityTypeBuilder;
        }

        /// <summary>
        ///     Sets whether the <see cref="IEntityType"/> being built is a root type of a polymorphic hierarchy.
        /// </summary>
        /// <param name="entityTypeBuilder">The <see cref="EntityTypeBuilder"/> to annotate.</param>
        /// <param name="isRootType">
        ///     <code>true</code> if the <see cref="IEntityType"/> is the root entity type; otherwise <code>false</code>.
        /// </param>
        /// <returns>The <paramref name="entityTypeBuilder"/>, such that calls be chained.</returns>
        public static EntityTypeBuilder ForMongoDbIsRootType(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            bool isRootType)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            entityTypeBuilder.MongoDb().IsRootType = isRootType;
            return entityTypeBuilder;
        }

        /// <summary>
        ///     Sets whether the identity of the <see cref="IEntityType"/> being built should be assigned by MongoDb on insert.
        /// </summary>
        /// <param name="entityTypeBuilder">The <see cref="EntityTypeBuilder"/> to annotate.</param>
        /// <param name="assignIdOnInsert">
        ///     <code>true</code> if the identity of the <see cref="IEntityType"/> is assigned on insert;
        ///     otherwise <code>false</code>.
        /// </param>
        /// <returns>The <paramref name="entityTypeBuilder"/>, such that calls be chained.</returns>
        public static EntityTypeBuilder ForMongoDbAssignIdOnInsert(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            bool assignIdOnInsert)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            entityTypeBuilder.MongoDb().AssignIdOnInsert = assignIdOnInsert;
            return entityTypeBuilder;
        }
    }
}