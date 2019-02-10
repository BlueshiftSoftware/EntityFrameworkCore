using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Blueshift.EntityFrameworkCore.MongoDB.Metadata.Builders;
using Blueshift.EntityFrameworkCore.MongoDB.ValueGeneration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.MongoDB.Metadata.Conventions
{
    /// <inheritdoc cref="IBaseTypeChangedConvention"/>
    /// <inheritdoc cref="IEntityTypeAddedConvention"/>
    /// <inheritdoc cref="IForeignKeyAddedConvention"/>
    /// <inheritdoc cref="IKeyAddedConvention"/>
    /// <inheritdoc cref="IKeyRemovedConvention"/>
    /// <inheritdoc cref="IModelBuiltConvention"/>
    public class OwnedDocumentConvention :
        IBaseTypeChangedConvention,
        IEntityTypeAddedConvention,
        IForeignKeyAddedConvention,
        IKeyAddedConvention,
        IKeyRemovedConvention,
        IModelBuiltConvention
    {
        /// <inheritdoc />
        public InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));

            EntityType entityType = entityTypeBuilder.Metadata;
            Key primaryKey = entityType.FindPrimaryKey();

            bool isComplexType = primaryKey == null
                                 || primaryKey.Document().IsOwnershipKey;

            entityTypeBuilder.Document().IsComplexType = isComplexType;

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
        public InternalRelationshipBuilder Apply(InternalRelationshipBuilder relationshipBuilder)
        {
            Check.NotNull(relationshipBuilder, nameof(relationshipBuilder));

            IEntityType principalEntityType = relationshipBuilder.Metadata.PrincipalEntityType;

            bool principalIsOwned = principalEntityType.IsOwned()
                                    || principalEntityType.MongoDb().IsComplexType;

            if (principalIsOwned)
            {
                relationshipBuilder.IsOwnership(true, ConfigurationSource.Explicit);
            }

            return relationshipBuilder;
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

                if (ownedEntityType.BaseType == null)
                {
                    IKey primaryKey = ownedEntityType.FindPrimaryKey();
                    string ownershipKeyName = $"{ownedEntityType.ShortName()}Id";

                    if (primaryKey != null && primaryKey.Properties.Any(property => !property.IsShadowProperty))
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
                                ConfigurationSource.Convention)
                            .IsDocumentOwnershipKey(true);
                    }
                }
            }

            return modelBuilder;
        }
    }
}
