using System;
using System.Collections.Generic;
using System.Linq;
using Blueshift.EntityFrameworkCore.MongoDB.Metadata.Builders;
using Blueshift.EntityFrameworkCore.MongoDB.ValueGeneration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.MongoDB.Metadata.Conventions
{
    /// <inheritdoc cref="IEntityTypeAddedConvention"/>
    /// <inheritdoc cref="IBaseTypeChangedConvention"/>
    /// <inheritdoc cref="IKeyAddedConvention"/>
    /// <inheritdoc cref="IKeyRemovedConvention"/>
    /// <inheritdoc cref="INavigationAddedConvention"/>
    /// <inheritdoc cref="INavigationRemovedConvention"/>
    /// <inheritdoc cref="IModelBuiltConvention"/>
    public class OwnedDocumentConvention :
        IEntityTypeAddedConvention,
        IBaseTypeChangedConvention,
        IKeyAddedConvention,
        IKeyRemovedConvention,
        IModelBuiltConvention
    {
        /// <inheritdoc />
        public InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            if (entityTypeBuilder.Metadata.FindPrimaryKey() == null)
            {
                entityTypeBuilder.MongoDb().IsComplexType = true;
            }

            return entityTypeBuilder;
        }

        /// <inheritdoc />
        public bool Apply(
            InternalEntityTypeBuilder entityTypeBuilder,
            EntityType oldBaseType)
        {
            Apply(entityTypeBuilder);

            return true;
        }

        /// <inheritdoc />
        public InternalKeyBuilder Apply(InternalKeyBuilder keyBuilder)
        {
            Apply(Check.NotNull(keyBuilder, nameof(keyBuilder)).Metadata.DeclaringEntityType.Builder);

            return keyBuilder;
        }

        /// <inheritdoc />
        public void Apply(InternalEntityTypeBuilder entityTypeBuilder, Key key)
        {
            Apply(entityTypeBuilder);
        }

        /// <inheritdoc />
        public InternalModelBuilder Apply(InternalModelBuilder modelBuilder)
        {
            IModel model = Check.NotNull(modelBuilder, nameof(modelBuilder)).Metadata;

            IEnumerable<EntityType> ownedEntityTypes = model
                .GetEntityTypes()
                .Where(entityType => entityType.IsOwned()
                                     || entityType.MongoDb().IsComplexType)
                .Select(entityType => entityType.AsEntityType())
                .ToList();

            foreach (EntityType ownedEntityType in ownedEntityTypes)
            {
                bool isOwnedType = ownedEntityType.HasClrType()
                    ? model.ShouldBeOwnedType(ownedEntityType.ClrType)
                    : model.ShouldBeOwnedType(ownedEntityType.Name);

                if (!isOwnedType)
                {
                    if (ownedEntityType.HasClrType())
                    {
                        modelBuilder.Owned(ownedEntityType.ClrType, ConfigurationSource.Convention);
                    }
                    else
                    {
                        modelBuilder.Owned(ownedEntityType.Name, ConfigurationSource.Convention);
                    }
                }

                IKey primaryKey = ownedEntityType.FindPrimaryKey();
                string ownershipKeyName = $"{ownedEntityType.ShortName()}Id";

                if (primaryKey != null && !primaryKey.Properties.All(property => property.IsShadowProperty))
                {
                    throw new InvalidOperationException(
                        $"Owned entity type {ownedEntityType.Name} has a non-shadow primary key defined.");
                }

                if (primaryKey == null || !string.Equals(ownershipKeyName, primaryKey.Properties.First().Name))
                {
                    ownedEntityType.Builder
                        .Property(
                            ownershipKeyName,
                            typeof(int?),
                            ConfigurationSource.Convention)
                        .HasValueGenerator(
                            typeof(HashCodeValueGenerator),
                            ConfigurationSource.Convention);

                    ownedEntityType.Builder
                        .PrimaryKey(
                            new[] { ownershipKeyName },
                            ConfigurationSource.Convention);
                }
            }

            return modelBuilder;
        }
    }
}
