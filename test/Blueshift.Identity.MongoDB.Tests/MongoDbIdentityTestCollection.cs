using Blueshift.MongoDB.Tests.Shared;
using Xunit;

namespace Blueshift.Identity.MongoDB.Tests
{
    [CollectionDefinition("MongoDB.Identity.Tests")]
    public class MongoDdIdentityTestCollection : ICollectionFixture<MongoDbFixture>
    {
    }
}
