using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;

namespace Blueshift.EntityFrameworkCore.Query
{
    public class MongoDbQueryContextFactory : QueryContextFactory
    {
        public MongoDbQueryContextFactory(
            [NotNull] QueryContextDependencies queryContextDependencies)
            : base(queryContextDependencies)
        {
        }

        public override QueryContext Create()
            => new MongoDbQueryContext(Dependencies, CreateQueryBuffer);
    }
}