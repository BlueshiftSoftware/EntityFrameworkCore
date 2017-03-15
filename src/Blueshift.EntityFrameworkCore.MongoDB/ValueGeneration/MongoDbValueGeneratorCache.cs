using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Blueshift.EntityFrameworkCore.ValueGeneration
{
    public class MongoDbValueGeneratorCache : ValueGeneratorCache
    {
        public MongoDbValueGeneratorCache([NotNull] ValueGeneratorCacheDependencies dependencies) : base(dependencies)
        {
        }
    }
}