using System;
using System.Linq;
using System.Threading;
using Blueshift.MongoDB.Tests.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Blueshift.Identity.MongoDB.Tests
{
    public class MongoDbUserRoleStoreTests : MongoDbIdentityStoreTestBase
    {
        private static readonly string RoleName1 = nameof(RoleName1);
        private static readonly string NormalizedRoleName1 = RoleName1.ToUpper();
        private static readonly string RoleName2 = nameof(RoleName2);
        private static readonly string NormalizedRoleName2 = RoleName2.ToUpper();

        private IUserRoleStore<MongoDbIdentityUser> _mongoDbUserRoleStore;

        public MongoDbUserRoleStoreTests(MongoDbFixture mongoDbFixture)
            : base(mongoDbFixture)
        {
            _mongoDbUserRoleStore = _serviceProvider.GetRequiredService<IUserRoleStore<MongoDbIdentityUser>>();
            var roleStore = _serviceProvider
                .GetRequiredService<IRoleStore<MongoDbIdentityRole>>();
            if (roleStore.FindByNameAsync(NormalizedRoleName1, new CancellationToken()).Result == null)
            {
                roleStore.CreateAsync(new MongoDbIdentityRole
                {
                    RoleName = RoleName1,
                    NormalizedRoleName = NormalizedRoleName1
                }, new CancellationToken())
                .Wait();
            }
            if (roleStore.FindByNameAsync(NormalizedRoleName2, new CancellationToken()).Result == null)
            {
                roleStore.CreateAsync(new MongoDbIdentityRole
                {
                    RoleName = RoleName2,
                    NormalizedRoleName = NormalizedRoleName2
                }, new CancellationToken())
                .Wait();
            }
        }

        protected override MongoDbIdentityUser CreateUser()
        {
            var user = base.CreateUser();
            user.Roles.Add(new MongoDbIdentityUserRole()
            {
                RoleName = RoleName2,
                NormalizedRoleName = NormalizedRoleName2
            });
            return user;
        }

        [Fact]
        public async void Can_add_to_role_async()
        {
            var user = CreateUser();
            await _mongoDbUserRoleStore.AddToRoleAsync(user, NormalizedRoleName1, new CancellationToken());
            var userRoles = user.Roles.Select(role => role.RoleName).ToList();
            Assert.Contains(RoleName1, userRoles, StringComparer.Ordinal);
            Assert.Contains(RoleName2, userRoles, StringComparer.Ordinal);
        }

        [Fact]
        public async void Can_get_roles_async()
        {
            var user = CreateUser();
            var roles = await _mongoDbUserRoleStore.GetRolesAsync(user, new CancellationToken());
            Assert.DoesNotContain(RoleName1, roles, StringComparer.Ordinal);
            Assert.Contains(RoleName2, roles, StringComparer.Ordinal);
        }

        [Fact]
        public async void Can_get_users_in_role_async()
        {
            var user = await CreateUserInDatabase();
            Assert.Equal(user, (await _mongoDbUserRoleStore.GetUsersInRoleAsync(NormalizedRoleName2, new CancellationToken())).Single(), UserComparer);
        }

        [Fact]
        public async void Can_remove_from_role_async()
        {
            var user = CreateUser();
            await _mongoDbUserRoleStore.RemoveFromRoleAsync(user, NormalizedRoleName2, new CancellationToken());
            Assert.Empty(user.Roles);
        }
    }
}