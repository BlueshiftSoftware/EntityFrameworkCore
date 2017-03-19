using Blueshift.EntityFrameworkCore.Metadata.Builders;
using Blueshift.EntityFrameworkCore.Metadata.Internal;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Bson.Serialization.Attributes;

namespace Blueshift.EntityFrameworkCore.Metadata.Conventions
{
    public class BsonDiscriminatorAttributeConvention : EntityTypeAttributeConvention<BsonDiscriminatorAttribute>
    {
        public override InternalEntityTypeBuilder Apply([NotNull] InternalEntityTypeBuilder entityTypeBuilder,
            [NotNull] BsonDiscriminatorAttribute attribute)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NotNull(attribute, nameof(attribute));
            MongoDbDocumentBuilder documentBuilder = entityTypeBuilder.MongoDb(ConfigurationSource.DataAnnotation);
            if (!string.IsNullOrWhiteSpace(attribute.Discriminator))
                documentBuilder.HasDiscriminator(attribute.Discriminator);
            documentBuilder.SetDiscriminatorIsRequired(attribute.Required);
            documentBuilder.SetIsRootType(attribute.RootClass);
            return entityTypeBuilder;
        }
    }
}