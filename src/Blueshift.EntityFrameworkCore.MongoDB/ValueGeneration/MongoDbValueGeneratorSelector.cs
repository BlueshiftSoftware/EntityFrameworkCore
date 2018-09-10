using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using MongoDB.Bson;

namespace Blueshift.EntityFrameworkCore.MongoDB.ValueGeneration
{
    /// <inheritdoc />
    public class MongoDbValueGeneratorSelector : ValueGeneratorSelector
    {
        private readonly IDictionary<Type, Func<ValueGenerator>> _valueGeneratorMap =
            new Dictionary<Type, Func<ValueGenerator>>
            {
                [typeof(ObjectId)] = () => new ObjectIdValueGenerator(),
                [typeof(short)] = () => new IntegerValueGenerator<short>(),
                [typeof(int)] = () => new IntegerValueGenerator<int>(),
                [typeof(long)] = () => new IntegerValueGenerator<long>(),
            };

        private readonly IDictionary<Type, Func<ValueGenerator>> _shadowKeyGeneratorMap =
            new Dictionary<Type, Func<ValueGenerator>>
            {
                [typeof(int)] = () => new HashCodeValueGenerator()
            };

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Blueshift.EntityFrameworkCore.MongoDB.ValueGeneration.MongoDbValueGeneratorSelector" /> class.
        /// </summary>
        /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
        public MongoDbValueGeneratorSelector([NotNull] ValueGeneratorSelectorDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <inheritdoc />
        /// <summary>
        ///     Creates a new value generator for the given property.
        /// </summary>
        /// <param name="property">The property to get the value generator for.</param>
        /// <param name="entityType">
        ///     The entity type that the value generator will be used for. When called on inherited
        ///     properties on derived entity types, this entity type may be different from the
        ///     declared entity type on property
        /// </param>
        /// <returns>The newly created value generator.</returns>
        public override ValueGenerator Create(
            IProperty property,
            IEntityType entityType)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(entityType, nameof(entityType));

            IDictionary<Type, Func<ValueGenerator>> valueGeneratorCreator = property.IsShadowProperty
                ? _shadowKeyGeneratorMap
                : _valueGeneratorMap;

            return valueGeneratorCreator
                .TryGetValue(
                    property.ClrType.UnwrapNullableType(),
                    out Func<ValueGenerator> valueGeneratorFactory)
                ? valueGeneratorFactory()
                : base.Create(property, entityType);
        }
    }
}