using System;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Identity;
using Xunit;

namespace Blueshift.Identity.MongoDB.Tests
{
    public class MongoDbUserAuthenticationTokenStoreTests : MongoDbIdentityStoreTestBase
    {
        private const string Token1Name = nameof(Token1Name);
        private const string Token2Name = nameof(Token2Name);
        private const string Token1Value = nameof(Token1Value);
        private const string Token2Value = nameof(Token2Value);

        private readonly IUserAuthenticationTokenStore<MongoDbIdentityUser> _mongoDbUserAuthenticationTokenStore;

        public MongoDbUserAuthenticationTokenStoreTests(MongoDbIdentityFixture mongoDbIdentityFixture)
            : base(mongoDbIdentityFixture)
        {
            _mongoDbUserAuthenticationTokenStore = mongoDbIdentityFixture.GetService<IUserAuthenticationTokenStore<MongoDbIdentityUser>>();
        }

        protected override MongoDbIdentityUser CreateUser()
        {
            var user = base.CreateUser();
            user.Logins.Add(new MongoDbIdentityUserLogin
            {
                LoginProvider = "Google",
                ProviderKey = new Guid().ToString(),
                ProviderDisplayName = "Google+",
                UserTokens =
                {
                    new MongoDbIdentityUserToken() { Name = Token1Name, Value = Token1Value },
                    new MongoDbIdentityUserToken() { Name = Token2Name, Value = Token2Value }
                }
            });
            return user;
        }

        [Fact]
        public async void Can_get_token_async()
        {
            var user = CreateUser();
            Assert.Equal(Token1Value, await _mongoDbUserAuthenticationTokenStore.GetTokenAsync(user, "Google", Token1Name, new CancellationToken()), StringComparer.Ordinal);
            Assert.Equal(Token2Value, await _mongoDbUserAuthenticationTokenStore.GetTokenAsync(user, "Google", Token2Name, new CancellationToken()), StringComparer.Ordinal);
            Assert.Null(await _mongoDbUserAuthenticationTokenStore.GetTokenAsync(user, "Google", "Token3", new CancellationToken()));
            Assert.Null(await _mongoDbUserAuthenticationTokenStore.GetTokenAsync(user, "Facebook", Token1Name, new CancellationToken()));
        }

        [Fact]
        public async void Can_set_token_async()
        {
            var user = CreateUser();
            string newTokenValue = nameof(newTokenValue);
            await _mongoDbUserAuthenticationTokenStore.SetTokenAsync(user, "Google", Token1Name, newTokenValue, new CancellationToken());
            Assert.Equal(newTokenValue, user.Logins
                   .First(login => login.LoginProvider == "Google")
                   .UserTokens
                   .First(userToken => userToken.Name == Token1Name).Value,
                StringComparer.Ordinal);
        }

        [Fact]
        public async void Can_remove_token_async()
        {
            var user = CreateUser();
            await _mongoDbUserAuthenticationTokenStore.RemoveTokenAsync(user, "Google", Token1Name, new CancellationToken());
            var userTokens = user.Logins
                   .First(login => login.LoginProvider == "Google")
                   .UserTokens;
            Assert.DoesNotContain(userTokens, userToken => userToken.Name == Token1Name);
            Assert.Equal(Token2Value,
                userTokens.First(userToken => userToken.Name == Token2Name).Value,
                StringComparer.Ordinal);
        }
    }
}