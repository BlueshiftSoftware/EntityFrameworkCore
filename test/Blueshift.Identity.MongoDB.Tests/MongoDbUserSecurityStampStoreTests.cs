using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Xunit;

namespace Blueshift.Identity.MongoDB.Tests
{
    public class MongoDbUserSecurityStampStoreTests : MongoDbIdentityStoreTestBase
    {
        private static readonly string SecurityStamp = new Guid().ToString();

        private readonly IUserSecurityStampStore<MongoDbIdentityUser> _mongoDbUserSecurityStampStore;

        public MongoDbUserSecurityStampStoreTests(MongoDbIdentityFixture mongoDbIdentityFixture)
            : base(mongoDbIdentityFixture)
        {
            _mongoDbUserSecurityStampStore = mongoDbIdentityFixture.GetService<IUserSecurityStampStore<MongoDbIdentityUser>>();
        }

        protected override MongoDbIdentityUser CreateUser()
        {
            var user = base.CreateUser();
            user.SecurityStamp = SecurityStamp;
            return user;
        }

        [Fact]
        public async Task Can_get_security_stamp_async()
        {
            var user = CreateUser();
            Assert.Equal(SecurityStamp, await _mongoDbUserSecurityStampStore.GetSecurityStampAsync(user, new CancellationToken()), StringComparer.Ordinal);
        }

        [Fact]
        public async Task Can_set_security_stamp_async()
        {
            var user = CreateUser();
            string newSecurityStamp = new Guid().ToString();
            await _mongoDbUserSecurityStampStore.SetSecurityStampAsync(user, newSecurityStamp, new CancellationToken());
            Assert.Equal(newSecurityStamp, user.SecurityStamp, StringComparer.Ordinal);
        }
    }
}