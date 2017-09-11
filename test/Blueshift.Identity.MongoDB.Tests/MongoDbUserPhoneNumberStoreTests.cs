using System;
using System.Threading;
using Blueshift.MongoDB.Tests.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Blueshift.Identity.MongoDB.Tests
{
    public class MongoDbUserPhoneNumberStoreTests : MongoDbIdentityStoreTestBase
    {
        private const string PhoneNumber = "+1.123.456.7890";

        private readonly IUserPhoneNumberStore<MongoDbIdentityUser> _mongoDbUserPhoneNumberStore;

        public MongoDbUserPhoneNumberStoreTests(MongoDbFixture mongoDbFixture)
            : base(mongoDbFixture)
        {
            _mongoDbUserPhoneNumberStore = ServiceProvider.GetRequiredService<IUserPhoneNumberStore<MongoDbIdentityUser>>();
        }

        protected override MongoDbIdentityUser CreateUser()
        {
            var user = base.CreateUser();
            user.PhoneNumber = PhoneNumber;
            user.PhoneNumberConfirmed = true;
            return user;
        }

        [Fact]
        public async void Can_get_phone_number_async()
        {
            var user = CreateUser();
            Assert.Equal(PhoneNumber, await _mongoDbUserPhoneNumberStore.GetPhoneNumberAsync(user, new CancellationToken()));
        }

        [Fact]
        public async void Can_set_phone_number_async()
        {
            var user = CreateUser();
            string newPhoneNumber = "+1.987.654.3210";
            await _mongoDbUserPhoneNumberStore.SetPhoneNumberAsync(user, newPhoneNumber, new CancellationToken());
            Assert.Equal(newPhoneNumber, user.PhoneNumber, StringComparer.Ordinal);
        }

        [Fact]
        public async void Can_check_phone_number_confirmed_async()
        {
            var user = CreateUser();
            user.PhoneNumberConfirmed = false;
            Assert.False(await _mongoDbUserPhoneNumberStore.GetPhoneNumberConfirmedAsync(user, new CancellationToken()));
            user.PhoneNumberConfirmed = true;
            Assert.True(await _mongoDbUserPhoneNumberStore.GetPhoneNumberConfirmedAsync(user, new CancellationToken()));
        }

        [Fact]
        public async void Can_set_phone_number_confirmed_async()
        {
            var user = CreateUser();
            await _mongoDbUserPhoneNumberStore.SetPhoneNumberConfirmedAsync(user, false, new CancellationToken());
            Assert.False(user.PhoneNumberConfirmed);
            await _mongoDbUserPhoneNumberStore.SetPhoneNumberConfirmedAsync(user, true, new CancellationToken());
            Assert.True(user.PhoneNumberConfirmed);
        }
    }
}