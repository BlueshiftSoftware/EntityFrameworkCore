using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Xunit;

namespace Blueshift.Identity.MongoDB.Tests
{
    [Collection("MongoDB.Identity.Tests")]
    public abstract class MongoDbIdentityStoreTestBase
    {
        protected static readonly string RoleName = "TestRole";
        protected static readonly string RoleNameNormalized = RoleName.ToUpper();

        protected static readonly string UserName = "user.name@domain.com";
        protected static readonly string UserNameNormalized = UserName.ToUpper();

        private readonly IUserStore<MongoDbIdentityUser> _userStore;
        private readonly IRoleStore<MongoDbIdentityRole> _roleStore;

        protected MongoDbIdentityStoreTestBase(MongoDbIdentityFixture mongoDbIdentityFixture)
        {
            _userStore = mongoDbIdentityFixture.GetService<IUserStore<MongoDbIdentityUser>>();
            _roleStore = mongoDbIdentityFixture.GetService<IRoleStore<MongoDbIdentityRole>>();
        }

        protected virtual MongoDbIdentityRole CreateRole()
            => new MongoDbIdentityRole
            {
                RoleName = RoleName,
                NormalizedRoleName = RoleNameNormalized
            };

        protected async Task<MongoDbIdentityRole> CreateRoleInDatabase()
        {
            MongoDbIdentityRole role = CreateRole();
            Assert.Equal(IdentityResult.Success, await _roleStore.CreateAsync(role, new CancellationToken()));
            return role;
        }

        protected virtual MongoDbIdentityUser CreateUser()
            => new MongoDbIdentityUser
            {
                UserName = UserName,
                NormalizedUserName = UserNameNormalized
            };

        protected async Task<MongoDbIdentityUser> CreateUserInDatabase()
        {
            MongoDbIdentityUser user = CreateUser();
            Assert.Equal(IdentityResult.Success, await _userStore.CreateAsync(user, new CancellationToken()));
            return user;
        }
    }
}