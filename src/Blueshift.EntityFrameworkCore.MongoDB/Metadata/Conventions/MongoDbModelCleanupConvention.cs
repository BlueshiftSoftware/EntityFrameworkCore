using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Blueshift.EntityFrameworkCore.MongoDB.Metadata.Conventions
{
    /// <inheritdoc />
    public class MongoDbModelCleanupConvention : ModelCleanupConvention
    {
        /// <inheritdoc />
        public override InternalModelBuilder Apply(InternalModelBuilder internalModelBuilder)
        {
            MarkComplexTypes(internalModelBuilder);
            return base.Apply(internalModelBuilder);
        }

        private static void MarkComplexTypes(InternalModelBuilder internalModelBuilder)
        {
            IEnumerable<EntityType> complexEntityTypes = internalModelBuilder
                .Metadata
                .GetEntityTypes()
                .Where(entityType => IsComplexType(entityType)
                                     && entityType.GetDerivedTypes().All(IsComplexType))
                .ToList();

            foreach (EntityType complexEntityType in complexEntityTypes)
            {
                complexEntityType.MongoDb().IsComplexType = true;

                if (complexEntityType.BaseType == null)
                {
                    InternalEntityTypeBuilder internalEntityTypeBuilder = complexEntityType.Builder;
                    InternalPropertyBuilder primaryKeyProperty = internalEntityTypeBuilder
                        .Property(
                            $"{complexEntityType.Name}TempId",
                            typeof(int),
                            ConfigurationSource.Convention);
                    primaryKeyProperty.ValueGenerated(ValueGenerated.OnAdd, ConfigurationSource.Convention);
                    internalEntityTypeBuilder.PrimaryKey(
                        new[] { primaryKeyProperty.Metadata.Name },
                        ConfigurationSource.Convention);
                }

                IEnumerable<ForeignKey> referencingForeignKeys = internalModelBuilder
                    .Metadata
                    .GetEntityTypes()
                    .SelectMany(entityType => entityType.GetForeignKeys())
                    .Where(foreignKey => foreignKey.PrincipalToDependent?.GetTargetType() == complexEntityType)
                    .Concat(complexEntityType.GetReferencingForeignKeys())
                    .ToList();

                foreach (ForeignKey referencingForeignKey in referencingForeignKeys)
                {
                    referencingForeignKey.IsOwnership = true;
                }
            }
        }

        private static bool IsComplexType(EntityType entityType)
            => entityType.FindPrimaryKey() == null;
    }
}
