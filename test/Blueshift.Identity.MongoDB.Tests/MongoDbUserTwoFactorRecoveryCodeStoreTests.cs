using System;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Identity;
using Xunit;

namespace Blueshift.Identity.MongoDB.Tests
{
    public class MongoDbUserTwoFactorRecoveryCodeStoreTests : MongoDbIdentityStoreTestBase
    {
        private const string RecoveryCode1 = nameof(RecoveryCode1);
        private const string RecoveryCode2 = nameof(RecoveryCode2);
        private const string RecoveryCode3 = nameof(RecoveryCode3);
        private static readonly string[] RecoveryCodes = new string[]
        {
            RecoveryCode1, RecoveryCode2, RecoveryCode3
        };

        private readonly IUserTwoFactorRecoveryCodeStore<MongoDbIdentityUser> _mongoDbUserTwoFactorRecoveryCodeStore;

        public MongoDbUserTwoFactorRecoveryCodeStoreTests(MongoDbIdentityFixture mongoDbIdentityFixture)
            : base(mongoDbIdentityFixture)
        {
            _mongoDbUserTwoFactorRecoveryCodeStore = mongoDbIdentityFixture.GetService<IUserTwoFactorRecoveryCodeStore<MongoDbIdentityUser>>();
        }

        protected override MongoDbIdentityUser CreateUser()
        {
            var user = base.CreateUser();
            user.Logins.Add(new MongoDbIdentityUserLogin
            {
                LoginProvider = "[BlueshiftMongoDbUserStore]",
                ProviderKey = new Guid().ToString(),
                ProviderDisplayName = "[BlueshiftMongoDbUserStore]",
                UserTokens =
                {
                    new MongoDbIdentityUserToken() { Name = "RecoveryCodes", Value = string.Join(";", RecoveryCodes) }
                }
            });
            return user;
        }

        [Theory]
        [InlineData(RecoveryCode1)]
        [InlineData(RecoveryCode2)]
        [InlineData(RecoveryCode3)]
        public async void Can_redeem_codes_exactly_once_async(string recoveryCode)
        {
            var user = CreateUser();
            Assert.True(await _mongoDbUserTwoFactorRecoveryCodeStore.RedeemCodeAsync(user, recoveryCode, new CancellationToken()));
            Assert.False(await _mongoDbUserTwoFactorRecoveryCodeStore.RedeemCodeAsync(user, recoveryCode, new CancellationToken()));
        }

        [Fact]
        public async void Can_replace_codes_async()
        {
            var user = CreateUser();
            var newRecoveryCodes = new [] { "New Code 1", "New Code 2", "New Code 3" };
            await _mongoDbUserTwoFactorRecoveryCodeStore.ReplaceCodesAsync(user, newRecoveryCodes, new CancellationToken());
            var recoveryCodes = user.Logins
                   .First(login => login.LoginProvider == "[BlueshiftMongoDbUserStore]")
                   .UserTokens
                   .First(userToken => userToken.Name == "RecoveryCodes").Value;
            Assert.DoesNotContain(RecoveryCode1, recoveryCodes, StringComparison.Ordinal);
            Assert.DoesNotContain(RecoveryCode2, recoveryCodes, StringComparison.Ordinal);
            Assert.DoesNotContain(RecoveryCode3, recoveryCodes, StringComparison.Ordinal);
            Assert.Equal(string.Join(";", newRecoveryCodes), recoveryCodes, StringComparer.Ordinal);
        }
    }
}