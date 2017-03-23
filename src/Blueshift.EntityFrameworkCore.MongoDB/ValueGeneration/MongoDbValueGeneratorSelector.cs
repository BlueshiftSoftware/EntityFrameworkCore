using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using MongoDB.Bson;

namespace Blueshift.EntityFrameworkCore.MongoDB.ValueGeneration
{
    /// <summary>
    ///     Selects value generators to be used to generate values for properties of entities.
    ///     This type is typically used by database providers (and other extensions). It
    ///     is generally not used in application code.
    /// </summary>
    public class MongoDbValueGeneratorSelector : ValueGeneratorSelector
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDbValueGeneratorSelector"/> class.
        /// </summary>
        /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
        public MongoDbValueGeneratorSelector([NotNull] ValueGeneratorSelectorDependencies dependencies)
            : base(dependencies)
        {
        }

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
        public override ValueGenerator Create([NotNull] IProperty property,
            [NotNull] IEntityType entityType)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(entityType, nameof(entityType));

            return property.ClrType.UnwrapNullableType().UnwrapEnumType() == typeof(ObjectId)
                ? new ObjectIdValueGenerator(property.ValueGenerated != ValueGenerated.Never)
                : base.Create(property, entityType);
        }
    }
}