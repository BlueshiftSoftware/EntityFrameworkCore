using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using MongoDB.Bson;

namespace Blueshift.EntityFrameworkCore.ValueGeneration
{
    public class MongoDbValueGeneratorSelector : ValueGeneratorSelector
    {
        public MongoDbValueGeneratorSelector([NotNull] ValueGeneratorSelectorDependencies dependencies) : base(dependencies)
        {
        }

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