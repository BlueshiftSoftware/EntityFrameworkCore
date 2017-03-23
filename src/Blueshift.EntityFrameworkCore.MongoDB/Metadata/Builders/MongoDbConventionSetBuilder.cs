using System.Collections.Generic;
using Blueshift.EntityFrameworkCore.MongoDB.Metadata.Conventions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.MongoDB.Metadata.Builders
{
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
        public MongoDbConventionSetBuilder([NotNull] MongoDbConventionSetBuilderDependencies mongoDbConventionSetBuilderDependencies)
        {
            _mongoDbConventionSetBuilderDependencies
                = Check.NotNull(mongoDbConventionSetBuilderDependencies, nameof(mongoDbConventionSetBuilderDependencies));
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConventionSet AddConventions([NotNull] ConventionSet conventionSet)
        {
            Check.NotNull(conventionSet, nameof(conventionSet));

            var mongoDatabaseAttributeConvention = new MongoDatabaseAttributeConvention(_mongoDbConventionSetBuilderDependencies.CurrentDbContext.Context);
            PropertyDiscoveryConvention mongoDbPropertyDiscoveryConvention = new MongoDbPropertyDiscoveryConvention();
            RelationshipDiscoveryConvention mongoDbRelationshipDiscoveryConvention = new MongoDbRelationshipDiscoveryConvention();
            PropertyMappingValidationConvention mongoDbPropertyMappingValidationConvention
                = new MongoDbPropertyMappingValidationConvention();
            DatabaseGeneratedAttributeConvention mongoDbDatabaseGeneratedAttributeConvention
                = new MongoDbDatabaseGeneratedAttributeConvention();

            conventionSet.ModelInitializedConventions
                .With(mongoDatabaseAttributeConvention);

            conventionSet.EntityTypeAddedConventions
                .Replace(mongoDbPropertyDiscoveryConvention)
                .Replace(mongoDbRelationshipDiscoveryConvention)
                .With(new BsonDiscriminatorAttributeConvention());

            conventionSet.BaseEntityTypeSetConventions
                .Replace(mongoDbPropertyDiscoveryConvention)
                .Replace(mongoDbRelationshipDiscoveryConvention);

            conventionSet.EntityTypeMemberIgnoredConventions
                .Replace(mongoDbRelationshipDiscoveryConvention);

            conventionSet.PropertyAddedConventions
                .Replace(mongoDbDatabaseGeneratedAttributeConvention)
                .With(new BsonIdAttributeConvention())
                .With(new BsonIgnoreAttributeConvention());

            conventionSet.PropertyFieldChangedConventions
                .Replace(mongoDbDatabaseGeneratedAttributeConvention);

            conventionSet.NavigationAddedConventions
                .Replace(mongoDbRelationshipDiscoveryConvention);

            conventionSet.NavigationRemovedConventions
                .Replace(mongoDbRelationshipDiscoveryConvention);

            conventionSet.ModelBuiltConventions
                .Replace(mongoDbPropertyMappingValidationConvention)
                .With(new MongoDbRegisterKnownTypesConvention());

            return conventionSet;
        }
    }
}