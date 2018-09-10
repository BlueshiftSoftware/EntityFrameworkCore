using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.MongoDB.Metadata.Conventions
{
    /// <inheritdoc />
    public class MongoDbForeignAddedConvention : IForeignKeyAddedConvention
    {
        /// <inheritdoc />
        public InternalRelationshipBuilder Apply(InternalRelationshipBuilder relationshipBuilder)
        {
            Check.NotNull(relationshipBuilder, nameof(relationshipBuilder));

            return relationshipBuilder;
        }
    }
}