using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Bson.Serialization.Attributes;

namespace Blueshift.EntityFrameworkCore.MongoDB.Metadata.Conventions
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class BsonDiscriminatorAttributeConvention : EntityTypeAttributeConvention<BsonDiscriminatorAttribute>
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override InternalEntityTypeBuilder Apply([NotNull] InternalEntityTypeBuilder entityTypeBuilder,
            [NotNull] BsonDiscriminatorAttribute attribute)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NotNull(attribute, nameof(attribute));
            MongoDbEntityTypeAnnotations annotations = entityTypeBuilder.MongoDb();
            if (!string.IsNullOrWhiteSpace(attribute.Discriminator))
                annotations.Discriminator = attribute.Discriminator;
            annotations.DiscriminatorIsRequired = attribute.Required;
            annotations.IsRootType = attribute.RootClass;
            return entityTypeBuilder;
        }
    }
}