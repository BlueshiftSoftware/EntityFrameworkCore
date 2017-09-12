using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;

namespace Blueshift.EntityFrameworkCore.MongoDB.Adapter.Conventions
{
    /// <summary>
    /// A convention that specifies that a discriminator is required when the given type is abstract.
    /// </summary>
    public class AbstractClassConvention : ConventionBase, IClassMapConvention
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractClassConvention" /> class.
        /// </summary>
        public AbstractClassConvention()
            : base(Regex.Replace(nameof(AbstractClassConvention), "Convention$", ""))
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
                classMap.SetDiscriminatorIsRequired(true);
            }
        }
    }
}