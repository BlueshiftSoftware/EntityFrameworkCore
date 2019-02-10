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
    public static class DocumentEntityTypeBuilderExtensions
    {
        /// <summary>
        ///     Sets whether the <see cref="IEntityType"/> is a complex type (i.e.: not a valid queryable root document entity).
        /// </summary>
        /// <param name="entityTypeBuilder">The <see cref="EntityTypeBuilder"/> to annotate.</param>
        /// <param name="isDocumentComplexType">
        ///     <code>true</code> if the <see cref="IEntityType"/> is a complex type;
        ///     otherwise <code>false</code>.
        /// </param>
        /// <returns>The <paramref name="entityTypeBuilder"/>, such that calls be chained.</returns>
        public static EntityTypeBuilder IsDocumentComplexType(
            [NotNull] this EntityTypeBuilder entityTypeBuilder,
            bool isDocumentComplexType)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            entityTypeBuilder.Document().IsComplexType = isDocumentComplexType;
            return entityTypeBuilder;
        }
    }
}