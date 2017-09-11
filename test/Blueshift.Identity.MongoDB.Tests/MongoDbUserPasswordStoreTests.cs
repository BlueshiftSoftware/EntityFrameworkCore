using System;
using System.Threading;
using Blueshift.MongoDB.Tests.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Blueshift.Identity.MongoDB.Tests
{
    public class MongoDbUserPasswordStoreTests : MongoDbIdentityStoreTestBase
    {
        private static readonly string PasswordHash = new Guid().ToString();

        private readonly IUserPasswordStore<MongoDbIdentityUser> _mongoDbUserPasswordStore;

        public MongoDbUserPasswordStoreTests(MongoDbFixture mongoDbFixture)
            : base(mongoDbFixture)
        {
            _mongoDbUserPasswordStore = ServiceProvider.GetRequiredService<IUserPasswordStore<MongoDbIdentityUser>>();
        }

        protected override MongoDbIdentityUser CreateUser()
        {
            var user = base.CreateUser();
            user.PasswordHash = PasswordHash;
            return user;
        }

        [Fact]
        public async void Can_get_password_hash_async()
        {
            var user = CreateUser();
            Assert.Equal(PasswordHash, await _mongoDbUserPasswordStore.GetPasswordHashAsync(user, new CancellationToken()), StringComparer.Ordinal);
        }

        [Fact]
        public async void Can_set_password_hash_async()
        {
            var user = CreateUser();
            var newPassword = new Guid().ToString();
            await _mongoDbUserPasswordStore.SetPasswordHashAsync(user, newPassword, new CancellationToken());
            Assert.Equal(newPassword, user.PasswordHash, StringComparer.Ordinal);
        }

        [Fact]
        public async void Can_check_has_password_async()
        {
            var user = CreateUser();
            Assert.True(await _mongoDbUserPasswordStore.HasPasswordAsync(user, new CancellationToken()));
            user.PasswordHash = null;
            Assert.False(await _mongoDbUserPasswordStore.HasPasswordAsync(user, new CancellationToken()));
        }
    }
}