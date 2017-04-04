using System;
using System.Threading;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Blueshift.Identity.MongoDB.Tests
{
    public class MongoDbUserTwoFactorStoreTests : MongoDbIdentityStoreTestBase
    {
        private static readonly string SecurityStamp = new Guid().ToString();

        private IUserTwoFactorStore<MongoDbIdentityUser> _mongoDbUserTwoFactorStore;

        public MongoDbUserTwoFactorStoreTests(MongoDbIdentityFixture mongoDbIdentityFixture)
            : base(mongoDbIdentityFixture)
        {
            _mongoDbUserTwoFactorStore = Services.GetRequiredService<IUserTwoFactorStore<MongoDbIdentityUser>>();
        }

        [Fact]
        public async void Can_check_two_factor_enabled_async()
        {
            var user = CreateUser();
            user.TwoFactorEnabled = false;
            Assert.False(await _mongoDbUserTwoFactorStore.GetTwoFactorEnabledAsync(user, new CancellationToken()));
            user.TwoFactorEnabled = true;
            Assert.True(await _mongoDbUserTwoFactorStore.GetTwoFactorEnabledAsync(user, new CancellationToken()));
        }

        [Fact]
        public async void Can_set_two_factor_enabled_async()
        {
            var user = CreateUser();
            await _mongoDbUserTwoFactorStore.SetTwoFactorEnabledAsync(user, false, new CancellationToken());
            Assert.False(user.TwoFactorEnabled);
            await _mongoDbUserTwoFactorStore.SetTwoFactorEnabledAsync(user, true, new CancellationToken());
            Assert.True(user.TwoFactorEnabled);
        }
    }
}