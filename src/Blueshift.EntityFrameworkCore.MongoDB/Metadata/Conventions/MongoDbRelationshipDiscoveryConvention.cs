using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Blueshift.EntityFrameworkCore.MongoDB.Metadata.Conventions
{
    /// <inheritdoc cref="RelationshipDiscoveryConvention"/>
    /// <inheritdoc cref="IForeignKeyOwnershipChangedConvention"/>
    public class MongoDbRelationshipDiscoveryConvention : RelationshipDiscoveryConvention, IForeignKeyOwnershipChangedConvention
    {
        /// <inheritdoc />
        public MongoDbRelationshipDiscoveryConvention(
            [NotNull] IMemberClassifier memberClassifier,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model> logger)
            : base(memberClassifier, logger)
        {
        }

        /// <inheritdoc />
        InternalRelationshipBuilder IForeignKeyOwnershipChangedConvention.Apply(InternalRelationshipBuilder relationshipBuilder)
        {
            Apply(relationshipBuilder.Metadata.DeclaringEntityType.Builder);
            return relationshipBuilder;
        }
    }
}
