using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.MongoDB.Metadata.Conventions
{
    /// <summary>
    /// A convention that specifies that a discriminator is required when the given type is abstract.
    /// </summary>
    public class MongoDbAbstractClassConvention : IEntityTypeAddedConvention
    {
        /// <summary>
        /// Applies the Abstract Class convention to the given <paramref name="entityTypeBuilder"/>.
        /// </summary>
        /// <param name="entityTypeBuilder">The <see cref="InternalEntityTypeBuilder" /> to which the convention will be applied.</param>
        /// <returns>The instance of <paramref name="entityTypeBuilder"/>, modified with this convention.</returns>
        public InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            if (entityTypeBuilder.Metadata.HasClrType() && entityTypeBuilder.Metadata.ClrType.GetTypeInfo().IsAbstract)
            {
                entityTypeBuilder
                    .MongoDb()
                    .DiscriminatorIsRequired = true;
            }
            return entityTypeBuilder;
        }
    }
}