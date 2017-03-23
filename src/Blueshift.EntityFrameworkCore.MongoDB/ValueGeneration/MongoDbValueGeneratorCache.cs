using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Blueshift.EntityFrameworkCore.MongoDB.ValueGeneration
{
    /// <summary>
    ///     Keeps a cache of value generators for properties.
    ///     This type is typically used by database providers (and other extensions). It
    ///     is generally not used in application code.
    /// </summary>
    public class MongoDbValueGeneratorCache : ValueGeneratorCache
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MongoDbValueGeneratorCache"/> class.
        /// </summary>
        /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
        public MongoDbValueGeneratorCache([NotNull] ValueGeneratorCacheDependencies dependencies) : base(dependencies)
        {
        }
    }
}