using System;
using System.Linq;
using System.Threading;
using Blueshift.MongoDB.Tests.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Blueshift.Identity.MongoDB.Tests
{
    public class MongoDbUserAuthenticatorKeyStoreTests : MongoDbIdentityStoreTestBase
    {
        private static readonly string AuthenticatorKey = new Guid().ToString();

        private readonly IUserAuthenticatorKeyStore<MongoDbIdentityUser> _mongoDbUserAuthenticatorKeyStore;

        public MongoDbUserAuthenticatorKeyStoreTests(MongoDbFixture mongoDbFixture)
            : base(mongoDbFixture)
        {
            _mongoDbUserAuthenticatorKeyStore = ServiceProvider.GetRequiredService<IUserAuthenticatorKeyStore<MongoDbIdentityUser>>();
        }

        protected override MongoDbIdentityUser CreateUser()
        {
            var user = base.CreateUser();
            user.Logins.Add(new MongoDbIdentityUserLogin
            {
                LoginProvider = "[BlueshiftMongoDbUserStore]",
                ProviderKey = new Guid().ToString(),
                ProviderDisplayName = "Blueshift MongoDb Provider",
                UserTokens =
                {
                    new MongoDbIdentityUserToken() { Name = "AuthenticatorKey", Value = AuthenticatorKey }
                }
            });
            return user;
        }

        [Fact]
        public async void Can_get_authenticator_key_async()
        {
            var user = CreateUser();
            Assert.Equal(AuthenticatorKey, await _mongoDbUserAuthenticatorKeyStore.GetAuthenticatorKeyAsync(user, new CancellationToken()), StringComparer.Ordinal);
        }

        [Fact]
        public async void Can_set_token_async()
        {
            var user = CreateUser();
            var newAuthenticatorKey = new Guid().ToString();
            await _mongoDbUserAuthenticatorKeyStore.SetAuthenticatorKeyAsync(user, newAuthenticatorKey, new CancellationToken());
            Assert.Equal(newAuthenticatorKey, user.Logins
                   .First(login => login.LoginProvider == "[BlueshiftMongoDbUserStore]")
                   .UserTokens
                   .First(userToken => userToken.Name == "AuthenticatorKey").Value,
                StringComparer.Ordinal);
        }
    }
}