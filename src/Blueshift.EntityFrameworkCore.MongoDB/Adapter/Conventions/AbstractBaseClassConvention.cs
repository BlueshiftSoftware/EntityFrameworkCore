using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Conventions;

namespace Blueshift.EntityFrameworkCore.MongoDB.Adapter.Conventions
{
    /// <inheritdoc cref="ConventionBase" />
    /// <inheritdoc cref="IClassMapConvention" />
    /// <summary>
    /// A convention that specifies that a discriminator is required when the given type is abstract.
    /// </summary>
    public class AbstractBaseClassConvention : BsonClassMapAttributeConvention<BsonKnownTypesAttribute>
    {
        /// <inheritdoc />
        /// <summary>
        /// Process the conventions on <paramref name="classMap"/> according to the given <paramref name="attribute"/>.
        /// </summary>
        /// <param name="classMap">The <see cref="BsonClassMap"/> to which the conventions will be assigned.</param>
        /// <param name="attribute">The <see cref="BsonKnownTypesAttribute" /> that defines the convention.</param>
        protected override void Apply(BsonClassMap classMap, BsonKnownTypesAttribute attribute)
        {
            Check.NotNull(classMap, nameof(classMap));
            if (!classMap.DiscriminatorIsRequired)
            {
                classMap.SetDiscriminatorIsRequired(classMap.ClassType.IsAbstract);
            }
        }
    }
}