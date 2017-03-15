using Blueshift.EntityFrameworkCore.Annotations;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.Metadata.Conventions
{
    public class MongoDbDiscriminatorAttributeConvention : EntityTypeAttributeConvention<DiscriminatorAttribute>
    {
        public override InternalEntityTypeBuilder Apply([NotNull] InternalEntityTypeBuilder entityTypeBuilder,
            [NotNull] DiscriminatorAttribute attribute)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NotNull(attribute, nameof(attribute));
            attribute.Apply(entityTypeBuilder);
            return entityTypeBuilder;
        }
    }
}