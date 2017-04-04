using System;
using System.Threading;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Blueshift.Identity.MongoDB.Tests
{
    public class MongoDbUserEmailStoreTests : MongoDbIdentityStoreTestBase
    {
        private const string EmailAddress = "test.user@domain.com";
        private static readonly string EmailAddressNormalized = EmailAddress.ToUpper();

        private IUserEmailStore<MongoDbIdentityUser> _mongoDbUserEmailStore;

        public MongoDbUserEmailStoreTests(MongoDbIdentityFixture mongoDbIdentityFixture)
            : base(mongoDbIdentityFixture)
        {
            _mongoDbUserEmailStore = Services.GetRequiredService<IUserEmailStore<MongoDbIdentityUser>>();
        }

        protected override MongoDbIdentityUser CreateUser()
        {
            var user = base.CreateUser();
            user.Email = EmailAddress;
            user.NormalizedEmail = EmailAddressNormalized;
            user.EmailConfirmed = false;
            return user;
        }

        [Fact]
        public async void Can_get_email_async()
        {
            var user = CreateUser();
            Assert.Equal(EmailAddress, await _mongoDbUserEmailStore.GetEmailAsync(user, new CancellationToken()), StringComparer.Ordinal);
        }

        [Fact]
        public async void Can_set_email_async()
        {
            var user = CreateUser();
            var newEmail = "new.email@address.com";
            await _mongoDbUserEmailStore.SetEmailAsync(user, newEmail, new CancellationToken());
            Assert.Equal(newEmail, user.Email, StringComparer.Ordinal);
        }

        [Fact]
        public async void Can_get_normalized_email_async()
        {
            var user = CreateUser();
            Assert.Equal(EmailAddressNormalized, await _mongoDbUserEmailStore.GetNormalizedEmailAsync(user, new CancellationToken()), StringComparer.Ordinal);
        }

        [Fact]
        public async void Can_set_normalized_email_async()
        {
            var user = CreateUser();
            var newEmailNormalized = "new.email@address.com".ToUpper();
            await _mongoDbUserEmailStore.SetNormalizedEmailAsync(user, newEmailNormalized, new CancellationToken());
            Assert.Equal(newEmailNormalized, user.NormalizedEmail, StringComparer.Ordinal);
        }

        [Fact]
        public async void Can_check_email_confirmed_async()
        {
            var user = CreateUser();
            user.EmailConfirmed = false;
            Assert.False(await _mongoDbUserEmailStore.GetEmailConfirmedAsync(user, new CancellationToken()));
            user.EmailConfirmed = true;
            Assert.True(await _mongoDbUserEmailStore.GetEmailConfirmedAsync(user, new CancellationToken()));
        }

        [Fact]
        public async void Can_set_email_confirmed_async()
        {
            var user = CreateUser();
            await _mongoDbUserEmailStore.SetEmailConfirmedAsync(user, false, new CancellationToken());
            Assert.False(user.EmailConfirmed);
            await _mongoDbUserEmailStore.SetEmailConfirmedAsync(user, true, new CancellationToken());
            Assert.True(user.EmailConfirmed);
        }

        [Fact]
        public async void Can_find_by_email_async()
        {
            var user = await CreateUserInDatabase();
            Assert.Equal(user, await _mongoDbUserEmailStore.FindByEmailAsync(EmailAddressNormalized, new CancellationToken()), UserComparer);
            Assert.Null(await _mongoDbUserEmailStore.FindByEmailAsync("DNE@EMAIL.COM", new CancellationToken()));
        }
    }
}