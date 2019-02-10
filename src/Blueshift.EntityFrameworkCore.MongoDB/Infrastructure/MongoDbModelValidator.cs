using System;
using System.Collections.Generic;
using System.Linq;
using Blueshift.EntityFrameworkCore.MongoDB.Metadata;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.MongoDB.Infrastructure
{
    /// <inheritdoc />
    /// <summary>
    ///     A validator that enforces rules for all MongoDb provider.
    /// </summary>
    public class MongoDbModelValidator : ModelValidator
    {
        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDbModelValidator"/> class.
        /// </summary>
        /// <param name="modelValidatorDependencies">Parameter object containing dependencies for this service.</param>
        public MongoDbModelValidator(
            [NotNull] ModelValidatorDependencies modelValidatorDependencies)
            : base(Check.NotNull(modelValidatorDependencies, nameof(modelValidatorDependencies)))
        {
        }

        /// <inheritdoc />
        /// <summary>
        ///     Validates a model, throwing an exception if any errors are found.
        /// </summary>
        /// <param name="model">The <see cref="Model"/> to validate.</param>
        public override void Validate(IModel model)
        {
            base.Validate(Check.NotNull(model, nameof(model)));

            EnsureDistinctCollectionNames(model);
            ValidateDerivedTypes(model);
        }

        /// <inheritdoc />
        protected override void ValidateNoShadowKeys(IModel model)
        {
            Check.NotNull(model, nameof(model));

            IEnumerable<IEntityType> nonComplexEntityTypes = model
                .GetEntityTypes()
                .Where(entityType => entityType.ClrType != null && !entityType.IsOwned());

            foreach (var entityType in nonComplexEntityTypes)
            {
                foreach (var key in entityType.GetDeclaredKeys())
                {
                    if (key.Properties.Any(p => p.IsShadowProperty)
                        && key is Key concreteKey
                        && ConfigurationSource.Convention.Overrides(concreteKey.GetConfigurationSource())
                        && !key.IsPrimaryKey())
                    {
                        var referencingFk = key.GetReferencingForeignKeys().FirstOrDefault();

                        if (referencingFk != null)
                        {
                            throw new InvalidOperationException(
                                CoreStrings.ReferencedShadowKey(
                                    referencingFk.DeclaringEntityType.DisplayName() +
                                    (referencingFk.DependentToPrincipal == null
                                        ? ""
                                        : "." + referencingFk.DependentToPrincipal.Name),
                                    entityType.DisplayName() +
                                    (referencingFk.PrincipalToDependent == null
                                        ? ""
                                        : "." + referencingFk.PrincipalToDependent.Name),
                                    Property.Format(referencingFk.Properties, includeTypes: true),
                                    Property.Format(entityType.FindPrimaryKey().Properties, includeTypes: true)));
                        }
                    }
                }
            }
        }

        /// <inheritdoc />
        protected override void ValidateOwnership(IModel model)
        {
            Check.NotNull(model, nameof(model));

            IList<IEntityType> ownedEntityTypes = model
                .GetEntityTypes()
                .Where(entityType => entityType.HasClrType()
                    ? model.ShouldBeOwnedType(entityType.ClrType)
                    : model.ShouldBeOwnedType(entityType.Name))
                .ToList();

            foreach (IEntityType entityType in ownedEntityTypes)
            {
                List<IForeignKey> ownerships = entityType
                    .GetForeignKeys()
                    .Where(fk => fk.IsOwnership)
                    .ToList();

                foreach (IForeignKey ownership in ownerships)
                {
                    IForeignKey principalToDependentForeignKey = entityType
                        .GetDeclaredForeignKeys()
                        .FirstOrDefault(foreignKey => !foreignKey.IsOwnership
                                                      && foreignKey.PrincipalToDependent != null);

                    if (principalToDependentForeignKey != null)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.InverseToOwnedType(
                                principalToDependentForeignKey.PrincipalEntityType.DisplayName(),
                                principalToDependentForeignKey.PrincipalToDependent.Name,
                                entityType.DisplayName(),
                                ownership.PrincipalEntityType.DisplayName()));
                    }
                }
            }
        }

        /// <summary>
        /// Ensures that each <see cref="EntityType"/> in the given <paramref name="model"/> has a unique collection name.
        /// </summary>
        /// <param name="model">The <see cref="Model"/> to validate.</param>
        protected virtual void EnsureDistinctCollectionNames([NotNull] IModel model)
        {
            Check.NotNull(model, nameof(model));
            var tables = new HashSet<string>();
            var duplicateCollectionNames = model
                .GetEntityTypes()
                .Where(et => et.BaseType == null)
                .Select(entityType => new
                {
                    new MongoDbEntityTypeAnnotations(entityType).CollectionName,
                    DisplayName = entityType.DisplayName()
                })
                .Where(tuple => !tables.Add(tuple.CollectionName));
            foreach (var tuple in duplicateCollectionNames)
            {
                throw new InvalidOperationException($"Duplicate collection name \"{tuple.CollectionName}\" defined on entity type \"{tuple.DisplayName}\".");
            }
        }

        /// <summary>
        /// Ensures that all entities in the given <paramref name="model"/> have unique discriminators.
        /// </summary>
        /// <param name="model">The <see cref="Model"/> to validate.</param>
        protected virtual void ValidateDerivedTypes([NotNull] IModel model)
        {
            IEnumerable<IEntityType> derivedTypes = Check.NotNull(model, nameof(model))
                .GetEntityTypes()
                .Where(entityType => entityType.BaseType != null && entityType.ClrType.IsInstantiable());
            var discriminatorSet = new HashSet<Tuple<IEntityType, string>>();
            foreach (IEntityType entityType in derivedTypes)
            {
                ValidateDiscriminator(entityType, discriminatorSet);
            }
        }

        private void ValidateDiscriminator(IEntityType entityType, ISet<Tuple<IEntityType,string>> discriminatorSet)
        {
            var annotations = new MongoDbEntityTypeAnnotations(entityType);
            if (string.IsNullOrWhiteSpace(annotations.Discriminator))
            {
                throw new InvalidOperationException($"Missing discriminator value for entity type {entityType.DisplayName()}.");
            }
            if (!discriminatorSet.Add(Tuple.Create(entityType.RootType(), annotations.Discriminator)))
            {
                throw new InvalidOperationException($"Duplicate discriminator value {annotations.Discriminator} for root entity type {entityType.RootType().DisplayName()} (defined on {entityType.DisplayName()}).");
            }
        }
    }
}