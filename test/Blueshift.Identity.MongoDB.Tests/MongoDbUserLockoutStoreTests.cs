using System;
using System.Threading;
using Blueshift.MongoDB.Tests.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Blueshift.Identity.MongoDB.Tests
{
    public class MongoDbUserLockoutStoreTests : MongoDbIdentityStoreTestBase
    {
        private static readonly DateTime LockoutEndDate = DateTime.Now.AddDays(1);

        private readonly IUserLockoutStore<MongoDbIdentityUser> _mongoDbUserLockoutStore;

        public MongoDbUserLockoutStoreTests(MongoDbFixture mongoDbFixture)
            : base(mongoDbFixture)
        {
            _mongoDbUserLockoutStore = ServiceProvider.GetRequiredService<IUserLockoutStore<MongoDbIdentityUser>>();
        }

        protected override MongoDbIdentityUser CreateUser()
        {
            var user = base.CreateUser();
            user.LockoutEnd = LockoutEndDate;
            return user;
        }

        [Fact]
        public async void Can_get_access_failed_count_async()
        {
            var user = CreateUser();
            Assert.Equal(user.AccessFailedCount, await _mongoDbUserLockoutStore.GetAccessFailedCountAsync(user, new CancellationToken()));
        }

        [Fact]
        public async void Can_check_lockout_enabled_async()
        {
            var user = CreateUser();
            user.LockoutEnabled = false;
            Assert.False(await _mongoDbUserLockoutStore.GetLockoutEnabledAsync(user, new CancellationToken()));
            user.LockoutEnabled = true;
            Assert.True(await _mongoDbUserLockoutStore.GetLockoutEnabledAsync(user, new CancellationToken()));
        }

        [Fact]
        public async void Can_set_lockout_enabled_async()
        {
            var user = CreateUser();
            await _mongoDbUserLockoutStore.SetLockoutEnabledAsync(user, false, new CancellationToken());
            Assert.False(user.LockoutEnabled);
            await _mongoDbUserLockoutStore.SetLockoutEnabledAsync(user, true, new CancellationToken());
            Assert.True(user.LockoutEnabled);
        }

        [Fact]
        public async void Can_get_lockout_end_date_async()
        {
            var user = CreateUser();
            Assert.Equal(user.LockoutEnd, await _mongoDbUserLockoutStore.GetLockoutEndDateAsync(user, new CancellationToken()));
        }

        [Fact]
        public async void Can_set_lockout_end_date_async()
        {
            var user = CreateUser();
            var lockoutEndDate = DateTime.Now.AddDays(3);
            await _mongoDbUserLockoutStore.SetLockoutEndDateAsync(user, lockoutEndDate, new CancellationToken());
            Assert.Equal(lockoutEndDate, user.LockoutEnd);
        }

        [Fact]
        public async void Can_reset_access_failed_count_async()
        {
            var user = CreateUser();
            user.AccessFailedCount = 10;
            await _mongoDbUserLockoutStore.ResetAccessFailedCountAsync(user, new CancellationToken());
            Assert.Equal(0, user.AccessFailedCount);
        }
    }
}