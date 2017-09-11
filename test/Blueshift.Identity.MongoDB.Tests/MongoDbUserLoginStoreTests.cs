using System;
using System.Linq;
using System.Threading;
using Blueshift.MongoDB.Tests.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Blueshift.Identity.MongoDB.Tests
{
    public class MongoDbUserLoginStoreTests : MongoDbIdentityStoreTestBase
    {
        private readonly IUserLoginStore<MongoDbIdentityUser> _mongoDbUserLoginStore;

        private const string ProviderName = nameof(ProviderName);
        private static readonly string ProviderKey = new Guid().ToString();
        private const string ProviderDisplayName = nameof(ProviderDisplayName);

        public MongoDbUserLoginStoreTests(MongoDbFixture mongoDbFixture)
            : base(mongoDbFixture)
        {
            _mongoDbUserLoginStore = ServiceProvider.GetRequiredService<IUserLoginStore<MongoDbIdentityUser>>();
        }

        protected override MongoDbIdentityUser CreateUser()
        {
            var user = base.CreateUser();
            user.Logins.Add(new MongoDbIdentityUserLogin
            {
                LoginProvider = ProviderName,
                ProviderKey = ProviderKey,
                ProviderDisplayName = ProviderDisplayName
            });
            return user;
        }

        [Fact]
        public async void Can_add_login_async()
        {
            var user = CreateUser();
            var userLoginInfo = new UserLoginInfo("Google", "1234567890abcdef", "Google+");
            await _mongoDbUserLoginStore.AddLoginAsync(user, userLoginInfo, new CancellationToken());
            Assert.NotNull(user.Logins
                .SingleOrDefault(identityUserLogin =>
                    userLoginInfo.LoginProvider == identityUserLogin.LoginProvider
                    && userLoginInfo.ProviderKey == identityUserLogin.ProviderKey
                    && userLoginInfo.ProviderDisplayName == identityUserLogin.ProviderDisplayName));
        }

        [Fact]
        public async void Can_remove_login_async()
        {
            var user = CreateUser();
            await _mongoDbUserLoginStore.RemoveLoginAsync(user, ProviderName, ProviderKey, new CancellationToken());
            Assert.Empty(user.Logins);
        }

        [Fact]
        public async void Can_find_by_login_async()
        {
            var user = await CreateUserInDatabase();
            Assert.Equal(user, await _mongoDbUserLoginStore.FindByLoginAsync(ProviderName, ProviderKey, new CancellationToken()), UserComparer);
        }

        [Fact]
        public async void Can_get_logins_async()
        {
            var user = CreateUser();
            var login = (await _mongoDbUserLoginStore.GetLoginsAsync(user, new CancellationToken())).Single();
            Assert.Equal(ProviderName, login.LoginProvider, StringComparer.Ordinal);
            Assert.Equal(ProviderKey, login.ProviderKey, StringComparer.Ordinal);
            Assert.Equal(ProviderDisplayName, login.ProviderDisplayName, StringComparer.Ordinal);
        }
    }
}