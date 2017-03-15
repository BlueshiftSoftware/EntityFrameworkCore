using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Blueshift.EntityFrameworkCore.Infrastructure
{
    public class MongoDbModelSource : ModelSource
    {
        public MongoDbModelSource([NotNull] ModelSourceDependencies modelSourceDependencies)
            : base(Check.NotNull(modelSourceDependencies, nameof(modelSourceDependencies)))
        {
        }
    }
}