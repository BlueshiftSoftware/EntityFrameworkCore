using Blueshift.EntityFrameworkCore.MongoDB.Metadata.Conventions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.MongoDB.Metadata.Builders
{
    /// <inheritdoc />
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class MongoDbConventionSetBuilder : IConventionSetBuilder
    {
        private readonly MongoDbConventionSetBuilderDependencies _mongoDbConventionSetBuilderDependencies;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MongoDbConventionSetBuilder" /> class.
        /// </summary>
        /// <param name="mongoDbConventionSetBuilderDependencies">Parameter object containing dependencies for this service.</param>
        public MongoDbConventionSetBuilder(
            [NotNull] MongoDbConventionSetBuilderDependencies mongoDbConventionSetBuilderDependencies)
        {
            _mongoDbConventionSetBuilderDependencies
                = Check.NotNull(mongoDbConventionSetBuilderDependencies, nameof(mongoDbConventionSetBuilderDependencies));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConventionSet AddConventions(ConventionSet conventionSet)
        {
            Check.NotNull(conventionSet, nameof(conventionSet));

            var mongoDbRelationshipDiscoveryConvention
                = new MongoDbRelationshipDiscoveryConvention(
                    _mongoDbConventionSetBuilderDependencies.MemberClassifier,
                    _mongoDbConventionSetBuilderDependencies.ModelLogger);
            RelationshipDiscoveryConvention relationshipDiscoveryConvention = mongoDbRelationshipDiscoveryConvention;

            var ownedDocumentConvention = new OwnedDocumentConvention();

            DatabaseGeneratedAttributeConvention databaseGeneratedAttributeConvention
                = new MongoDbDatabaseGeneratedAttributeConvention();

            KeyAttributeConvention keyAttributeConvention = new MongoDbKeyAttributeConvention();

            var mongoDatabaseConvention
                = new MongoDatabaseConvention(_mongoDbConventionSetBuilderDependencies.CurrentDbContext.Context);

            var bsonRequiredAttributeConvention
                = new BsonRequiredAttributeConvention();

            PropertyMappingValidationConvention propertyMappingValidationConvention
                = new DocumentPropertyMappingValidationConvention(
                    _mongoDbConventionSetBuilderDependencies.MongoDbTypeMapperSource,
                    _mongoDbConventionSetBuilderDependencies.MemberClassifier);
            
            conventionSet.ModelInitializedConventions
                .With(mongoDatabaseConvention);

            conventionSet.EntityTypeAddedConventions
                .Replace(relationshipDiscoveryConvention)
                .With(ownedDocumentConvention)
                .With(new MongoCollectionAttributeConvention())
                .With(new BsonDiscriminatorAttributeConvention())
                .With(new BsonIgnoreAttributeConvention())
                .With(new BsonKnownTypesAttributeConvention());

            conventionSet.BaseEntityTypeChangedConventions
                .Replace(relationshipDiscoveryConvention)
                .With(ownedDocumentConvention);

            conventionSet.EntityTypeMemberIgnoredConventions
                .Replace(relationshipDiscoveryConvention);

            conventionSet.KeyAddedConventions
                .With(ownedDocumentConvention);

            conventionSet.KeyRemovedConventions
                .With(ownedDocumentConvention);

            conventionSet.ForeignKeyAddedConventions
                .With(ownedDocumentConvention);

            conventionSet.ForeignKeyOwnershipChangedConventions
                .With(mongoDbRelationshipDiscoveryConvention)
                .Without(item => item is NavigationEagerLoadingConvention);

            conventionSet.PropertyAddedConventions
                .Replace(databaseGeneratedAttributeConvention)
                .Replace(keyAttributeConvention)
                .With(bsonRequiredAttributeConvention);

            conventionSet.PropertyFieldChangedConventions
                .Replace(databaseGeneratedAttributeConvention)
                .Replace(keyAttributeConvention)
                .With(bsonRequiredAttributeConvention);

            conventionSet.NavigationAddedConventions
                .Replace(relationshipDiscoveryConvention);

            conventionSet.NavigationRemovedConventions
                .Replace(relationshipDiscoveryConvention);

            conventionSet.ModelBuiltConventions
                .Replace(keyAttributeConvention)
                .Replace(propertyMappingValidationConvention)
                .With(ownedDocumentConvention);

            return conventionSet;
        }
    }
}