using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Blueshift.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using MongoDB.Bson.Serialization.Attributes;

// ReSharper disable once CheckNamespace
namespace Blueshift.EntityFrameworkCore.Infrastructure
{
    public class MongoDbModelValidator : ModelValidator
    {
        public MongoDbModelValidator(
            [NotNull] ModelValidatorDependencies modelValidatorDependencies)
            : base(Check.NotNull(modelValidatorDependencies, nameof(modelValidatorDependencies)))
        {
        }

        public override void Validate([NotNull] IModel model)
        {
            Check.NotNull(model, nameof(model));
            base.Validate(model);

            EnsureDistinctCollectionNames(model);
            EnsureKnownTypes(model);
            ValidateDerivedTypes(model);
        }

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
                ShowError($"Duplicate collection name \"{tuple.CollectionName}\" defined on entity type \"{tuple.DisplayName}\".");
            }
        }

        protected virtual void EnsureKnownTypes([NotNull] IModel model)
        {
            Check.NotNull(model, nameof(model));
            var unregisteredTypes = model
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
            InternalModelBuilder modelBuilder = model.AsModel().Builder;
            foreach (var pair in unregisteredTypes)
            {
                if (!pair.BaseType.GetTypeInfo().IsAssignableFrom(pair.DerivedType))
                    ShowError($"Known type {pair.DerivedType} declared on base type {pair.BaseType} does not inherit from base type.");
            }
        }

        protected virtual void ValidateDerivedTypes([NotNull] IModel model)
        {
            Check.NotNull(model, nameof(model));
            var discriminatorSet = new HashSet<Tuple<IEntityType,string>>();
            IEnumerable<IEntityType> derivedTypes = model.GetEntityTypes()
                .Where(entityType => entityType.BaseType != null && entityType.ClrType.IsInstantiable());
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
                ShowError($"Missing discriminator value for entity type {entityType.DisplayName()}.");
            }
            if (!discriminatorSet.Add(Tuple.Create(entityType.RootType(), annotations.Discriminator)))
            {
                ShowError($"Duplicate discriminator value {annotations.Discriminator} for root entity type {entityType.RootType().DisplayName()} (defined on {entityType.DisplayName()}).");
            }
        }
    }
}
