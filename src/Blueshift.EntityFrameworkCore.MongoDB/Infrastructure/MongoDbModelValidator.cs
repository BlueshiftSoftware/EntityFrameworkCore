using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Blueshift.EntityFrameworkCore.MongoDB.Metadata;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Bson.Serialization.Attributes;

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

            EnsureKnownTypes(model);
            EnsureDistinctCollectionNames(model);
            ValidateDerivedTypes(model);
        }

        /// <inheritdoc />
        protected override void ValidateNoShadowKeys(IModel model)
        {
            Check.NotNull(model, nameof(model));

            IEnumerable<IEntityType> nonComplexEntityTypes = model
                .GetEntityTypes()
                .Where(entityType => entityType.ClrType != null && !entityType.MongoDb().IsComplexType);

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
            }        }

        /// <inheritdoc />
        protected override void ValidateOwnership(IModel model)
        {
            Check.NotNull(model, nameof(model));

            foreach (IEntityType entityType in model.GetEntityTypes())
            {
                List<IForeignKey> ownerships = entityType.GetForeignKeys().Where(fk => fk.IsOwnership).ToList();
                if (ownerships.Count == 0
                    && entityType.HasClrType()
                        ? model.ShouldBeOwnedType(entityType.ClrType)
                        : model.ShouldBeOwnedType(entityType.Name))
                {
                    throw new InvalidOperationException(CoreStrings.OwnerlessOwnedType(entityType.DisplayName()));
                }

                foreach (IForeignKey ownership in ownerships)
                {
                    IEnumerable<IForeignKey> foreignKeys = entityType.GetDeclaredForeignKeys()
                        .Where(fk => !fk.IsOwnership && fk.PrincipalToDependent != null);

                    foreach (IForeignKey foreignKey in foreignKeys)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.InverseToOwnedType(
                                foreignKey.PrincipalEntityType.DisplayName(),
                                foreignKey.PrincipalToDependent.Name,
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
        /// Ensures that derived entity types declared in <see cref="BsonKnownTypesAttribute"/> are registered for each
        /// <see cref="EntityType"/> in the given <see cref="Model"/>.
        /// </summary>
        /// <param name="model">The <see cref="Model"/> to be ensured.</param>
        protected virtual void EnsureKnownTypes([NotNull] IModel model)
        {
            var unregisteredTypes = Check.NotNull(model, nameof(model))
                .GetEntityTypes()
                .Where(entityType => entityType.HasClrType()
                    && entityType.ClrType.GetTypeInfo().IsDefined(typeof(BsonKnownTypesAttribute), false))
                .SelectMany(entityType => entityType.ClrType.GetTypeInfo()
                    .GetCustomAttributes<BsonKnownTypesAttribute>(false)
                    .SelectMany(bsonKnownTypesAttribute => bsonKnownTypesAttribute.KnownTypes)
                    .Select(derivedType =>  new
                    {
                        BaseType = entityType.ClrType,
                        DerivedType = derivedType
                    }))
                .Where(tuple => model.FindEntityType(tuple.DerivedType.Name) == null)
                .ToList();
            foreach (var pair in unregisteredTypes)
            {
                if (!pair.BaseType.GetTypeInfo().IsAssignableFrom(pair.DerivedType))
                    throw new InvalidOperationException($"Known type {pair.DerivedType} declared on base type {pair.BaseType} does not inherit from base type.");
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