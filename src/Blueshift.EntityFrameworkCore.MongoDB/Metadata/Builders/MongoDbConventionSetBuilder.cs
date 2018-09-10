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

            DatabaseGeneratedAttributeConvention databaseGeneratedAttributeConvention
                = new MongoDbDatabaseGeneratedAttributeConvention();
            KeyAttributeConvention keyAttributeConvention = new MongoDbKeyAttributeConvention();
            var mongoDatabaseConvention
                = new MongoDatabaseConvention(_mongoDbConventionSetBuilderDependencies.CurrentDbContext.Context);
            ModelCleanupConvention modelCleanupConvention
                = new MongoDbModelCleanupConvention();

            conventionSet.ModelInitializedConventions
                .With(mongoDatabaseConvention);

            conventionSet.EntityTypeAddedConventions
                .InsertBefore<IEntityTypeAddedConvention, BaseTypeDiscoveryConvention>(new MongoDbBaseTypeDiscoveryConvention())
                .With(new MongoCollectionAttributeConvention())
                .With(new BsonIgnoreAttributeConvention())
                .With(new BsonDiscriminatorAttributeConvention())
                .With(new MongoDbRegisterKnownTypesConvention());

            conventionSet.ForeignKeyOwnershipChangedConventions
                .Without(item => item is NavigationEagerLoadingConvention);

            conventionSet.ForeignKeyAddedConventions
                .With(new MongoDbForeignAddedConvention());

            conventionSet.PropertyAddedConventions
                .Replace(databaseGeneratedAttributeConvention)
                .Replace(keyAttributeConvention);

            conventionSet.PropertyFieldChangedConventions
                .Replace(databaseGeneratedAttributeConvention)
                .Replace(keyAttributeConvention);

            conventionSet.ModelBuiltConventions
                .Replace(keyAttributeConvention)
                .Replace(modelCleanupConvention);

            return conventionSet;
        }
    }
}