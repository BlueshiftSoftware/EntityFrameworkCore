using System;
using System.Threading;
using Blueshift.MongoDB.Tests.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Blueshift.Identity.MongoDB.Tests
{
    public class MongoDbRoleStoreTests : MongoDbIdentityStoreTestBase
    {
        private IRoleStore<MongoDbIdentityRole> _mongoDbRoleStore;

        public MongoDbRoleStoreTests(MongoDbFixture mongoDbFixture)
            : base(mongoDbFixture)
        {
            _mongoDbRoleStore = _serviceProvider.GetRequiredService<IRoleStore<MongoDbIdentityRole>>();
        }

        [Fact]
        public async void Can_create_role_async()
        {
            Assert.NotNull(await CreateRoleInDatabase());
        }

        [Fact]
        public async void Can_delete_role_async()
        {
            var role = await CreateRoleInDatabase();
            Assert.Equal(IdentityResult.Success, await _mongoDbRoleStore.DeleteAsync(role, new CancellationToken()));
        }

        [Fact]
        public async void Can_find_by_id_async()
        {
            var role = await CreateRoleInDatabase();
            Assert.Equal(role, await _mongoDbRoleStore.FindByIdAsync(role.Id.ToString(), new CancellationToken()), RoleComparer);
        }

        [Fact]
        public async void Can_find_by_name_async()
        {
            var role = await CreateRoleInDatabase();
            Assert.Equal(role, await _mongoDbRoleStore.FindByNameAsync(role.NormalizedRoleName, new CancellationToken()), RoleComparer);
        }

        [Fact]
        public async void Can_get_normalized_role_name_async()
        {
            var role = CreateRole();
            Assert.Equal(role.NormalizedRoleName, await _mongoDbRoleStore.GetNormalizedRoleNameAsync(role, new CancellationToken()), StringComparer.Ordinal);
        }

        [Fact]
        public async void Can_get_role_id_async()
        {
            var role = await CreateRoleInDatabase();
            Assert.Equal(role.Id.ToString(), await _mongoDbRoleStore.GetRoleIdAsync(role, new CancellationToken()), StringComparer.Ordinal);
        }

        [Fact]
        public async void Can_get_user_name_async()
        {
            var role = CreateRole();
            Assert.Equal(role.RoleName, await _mongoDbRoleStore.GetRoleNameAsync(role, new CancellationToken()), StringComparer.Ordinal);
        }

        [Fact]
        public async void Can_set_normalized_role_name_async()
        {
            var role = await CreateRoleInDatabase();
            string newNormalizedRoleName = nameof(newNormalizedRoleName).ToUpper();
            await _mongoDbRoleStore.SetNormalizedRoleNameAsync(role, newNormalizedRoleName, new CancellationToken());
            Assert.Equal(newNormalizedRoleName, role.NormalizedRoleName, StringComparer.Ordinal);
        }

        [Fact]
        public async void Can_set_role_name_async()
        {
            var role = await CreateRoleInDatabase();
            string newRoleName = nameof(newRoleName);
            await _mongoDbRoleStore.SetRoleNameAsync(role, newRoleName, new CancellationToken());
            Assert.Equal(newRoleName, role.RoleName, StringComparer.Ordinal);
        }

        [Fact]
        public async void Can_update_role_async()
        {
            var role = await CreateRoleInDatabase();
            var newRoleName = "New.Role.Name";
            var newNormalizedRoleName = newRoleName.ToUpper();
            role.RoleName = newRoleName;
            role.NormalizedRoleName = newNormalizedRoleName;
            Assert.Equal(IdentityResult.Success, await _mongoDbRoleStore.UpdateAsync(role, new CancellationToken()));
            Assert.Equal(role, await _mongoDbRoleStore.FindByNameAsync(newNormalizedRoleName, new CancellationToken()), RoleComparer);
        }
    }
}