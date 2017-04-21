using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Blueshift.MongoDB.Tests.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Blueshift.Identity.MongoDB.Tests
{
    [Collection("MongoDB.Identity.Tests")]
    public abstract class MongoDbIdentityStoreTestBase : IDisposable
    {
        protected static readonly string RoleName = "TestRole";
        protected static readonly string RoleNameNormalized = RoleName.ToUpper();

        protected static readonly string UserName = "user.name@domain.com";
        protected static readonly string UserNameNormalized = UserName.ToUpper();

        protected static IEqualityComparer<MongoDbIdentityRole> RoleComparer
            => new FuncEqualityComparer<MongoDbIdentityRole>((role1, role2)
                => (role1.Id == role2.Id)
                    && string.Equals(role1.RoleName, role2.RoleName, StringComparison.Ordinal)
                    && string.Equals(role1.NormalizedRoleName, role2.NormalizedRoleName, StringComparison.Ordinal)
                    && EnumerableEqualityComparer<MongoDbIdentityClaim>.Equals(role1.Claims, role2.Claims, IdentityClaimComparer));

        protected static IEqualityComparer<MongoDbIdentityUser> UserComparer
            => new FuncEqualityComparer<MongoDbIdentityUser>((user1, user2)
                => (user1.Id == user2.Id)
                    && string.Equals(user1.UserName, user2.UserName, StringComparison.Ordinal)
                    && string.Equals(user1.NormalizedUserName, user2.NormalizedUserName, StringComparison.Ordinal)
                    && EnumerableEqualityComparer<MongoDbIdentityUserLogin>.Equals(user1.Logins, user2.Logins, UserLoginComparer)
                    && EnumerableEqualityComparer<MongoDbIdentityClaim>.Equals(user1.Claims, user2.Claims, IdentityClaimComparer));

        protected static IEqualityComparer<MongoDbIdentityUserLogin> UserLoginComparer
            => new FuncEqualityComparer<MongoDbIdentityUserLogin>((userLogin1, userLogin2)
                => string.Equals(userLogin1.LoginProvider, userLogin2.LoginProvider, StringComparison.Ordinal)
                    && string.Equals(userLogin1.ProviderKey, userLogin2.ProviderKey, StringComparison.Ordinal)
                    && string.Equals(userLogin1.ProviderDisplayName, userLogin2.ProviderDisplayName, StringComparison.Ordinal)
                    && EnumerableEqualityComparer<MongoDbIdentityUserToken>.Equals(userLogin1.UserTokens, userLogin2.UserTokens, UserTokenComparer));

        protected static IEqualityComparer<MongoDbIdentityUserToken> UserTokenComparer
            => new FuncEqualityComparer<MongoDbIdentityUserToken>((userToken1, userToken2)
                => string.Equals(userToken1.Name, userToken2.Name, StringComparison.Ordinal)
                    && string.Equals(userToken1.Value, userToken2.Value, StringComparison.Ordinal));

        protected static IEqualityComparer<MongoDbIdentityClaim> IdentityClaimComparer
            => new FuncEqualityComparer<MongoDbIdentityClaim>((claim1, claim2)
                => string.Equals(claim1.ClaimType, claim2.ClaimType, StringComparison.Ordinal)
                    && string.Equals(claim1.ClaimValue, claim2.ClaimValue, StringComparison.Ordinal));

        protected IServiceProvider _serviceProvider;
        private IdentityMongoDbContext _identityDbContext;
        private IUserStore<MongoDbIdentityUser> _userStore;
        private IRoleStore<MongoDbIdentityRole> _roleStore;

        protected MongoDbIdentityStoreTestBase(MongoDbFixture mongoDbFixture)
        {
            _serviceProvider = new ServiceCollection()
                .AddDbContext<IdentityMongoDbContext>(options => options
                    .UseMongoDb(connectionString: MongoDbConstants.MongoUrl)
                    .EnableSensitiveDataLogging(true))
                .AddIdentity<MongoDbIdentityUser, MongoDbIdentityRole>()
                .AddEntityFrameworkMongoDbStores<IdentityMongoDbContext>()
                .Services
                .BuildServiceProvider();
            _identityDbContext = _serviceProvider.GetService<IdentityMongoDbContext>();
            _userStore = _serviceProvider.GetRequiredService<IUserStore<MongoDbIdentityUser>>();
            _roleStore = _serviceProvider.GetRequiredService<IRoleStore<MongoDbIdentityRole>>();
            _identityDbContext.Database.EnsureCreated();
        }

        public virtual void Dispose()
        {
            if (_identityDbContext != null)
            {
                _identityDbContext.Database.EnsureDeleted();
                _identityDbContext = null;
            }
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