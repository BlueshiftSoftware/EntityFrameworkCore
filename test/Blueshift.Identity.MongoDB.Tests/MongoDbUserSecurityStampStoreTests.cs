using System;
using System.Threading;
using Blueshift.MongoDB.Tests.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Blueshift.Identity.MongoDB.Tests
{
    public class MongoDbUserSecurityStampStoreTests : MongoDbIdentityStoreTestBase
    {
        private static readonly string SecurityStamp = new Guid().ToString();

        private readonly IUserSecurityStampStore<MongoDbIdentityUser> _mongoDbUserSecurityStampStore;

        public MongoDbUserSecurityStampStoreTests(MongoDbFixture mongoDbFixture)
            : base(mongoDbFixture)
        {
            _mongoDbUserSecurityStampStore = ServiceProvider.GetRequiredService<IUserSecurityStampStore<MongoDbIdentityUser>>();
        }

        protected override MongoDbIdentityUser CreateUser()
        {
            var user = base.CreateUser();
            user.SecurityStamp = SecurityStamp;
            return user;
        }

        [Fact]
        public async void Can_get_security_stamp_async()
        {
            var user = CreateUser();
            Assert.Equal(SecurityStamp, await _mongoDbUserSecurityStampStore.GetSecurityStampAsync(user, new CancellationToken()), StringComparer.Ordinal);
        }

        [Fact]
        public async void Can_set_security_stamp_async()
        {
            var user = CreateUser();
            string newSecurityStamp = new Guid().ToString();
            await _mongoDbUserSecurityStampStore.SetSecurityStampAsync(user, newSecurityStamp, new CancellationToken());
            Assert.Equal(newSecurityStamp, user.SecurityStamp, StringComparer.Ordinal);
        }
    }
}