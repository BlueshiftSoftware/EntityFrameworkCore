using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Bson.Serialization.Attributes;

namespace Blueshift.EntityFrameworkCore.MongoDB.Metadata.Conventions
{
    /// <inheritdoc />
    public class BsonDiscriminatorAttributeConvention : EntityTypeAttributeConvention<BsonDiscriminatorAttribute>
    {
        /// <inheritdoc />
        public override InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder,
            BsonDiscriminatorAttribute attribute)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NotNull(attribute, nameof(attribute));
            MongoDbEntityTypeAnnotations annotations = entityTypeBuilder.MongoDb();
            if (!string.IsNullOrWhiteSpace(attribute.Discriminator))
            {
                annotations.Discriminator = attribute.Discriminator;
            }

            if (!annotations.DiscriminatorIsRequired)
            {
                annotations.DiscriminatorIsRequired = attribute.Required;
            }

            annotations.IsRootType = attribute.RootClass;
            return entityTypeBuilder;
        }
    }
}