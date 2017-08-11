using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;

namespace Blueshift.EntityFrameworkCore.MongoDB.Adapter
{
    /// <summary>
    /// A convention that specifies that a discriminator is required when the given type is abstract.
    /// </summary>
    public class AbstractClassConvention : ConventionBase, IClassMapConvention, IEntityTypeAddedConvention
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractClassConvention" /> class.
        /// </summary>
        public AbstractClassConvention()
            : base(Regex.Replace(nameof(AbstractClassConvention), pattern: "Convention$", replacement: ""))
        {
        }

        /// <summary>
        /// Applies the Abstract Class convention to the given <paramref name="classMap"/>.
        /// </summary>
        /// <param name="classMap">The <see cref="BsonClassMap" /> to which the convention will be applied.</param>
        public virtual void Apply([NotNull] BsonClassMap classMap)
        {
            Check.NotNull(classMap, nameof(classMap));
            if (classMap.ClassType.GetTypeInfo().IsAbstract)
            {
                classMap.SetDiscriminatorIsRequired(discriminatorIsRequired: true);
            }
        }

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