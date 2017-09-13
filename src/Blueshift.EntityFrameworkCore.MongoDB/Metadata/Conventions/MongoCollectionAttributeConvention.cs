using Blueshift.EntityFrameworkCore.MongoDB.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.MongoDB.Metadata.Conventions
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class MongoCollectionAttributeConvention : EntityTypeAttributeConvention<MongoCollectionAttribute>
    {
        /// <inheritdoc />
        public override InternalEntityTypeBuilder Apply(
            InternalEntityTypeBuilder entityTypeBuilder,
            MongoCollectionAttribute mongoCollectionAttribute)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NotNull(mongoCollectionAttribute, nameof(mongoCollectionAttribute));
            entityTypeBuilder.MongoDb().CollectionName = mongoCollectionAttribute.CollectionName;
            return entityTypeBuilder;
        }
    }
}
