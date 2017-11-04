using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Blueshift.MongoDB.Tests.Shared;
using JetBrains.Annotations;
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

        protected static IEqualityComparer<MongoDbIdentityRole> RoleComparer = new MongoDbIdentityRoleComparer();

        protected static IEqualityComparer<MongoDbIdentityUser> UserComparer = new MongoDbIdentityUserComparer();

        protected static IEqualityComparer<MongoDbIdentityUserLogin> UserLoginComparer = new MongoDbIdentityUserLoginComparer();

        protected static IEqualityComparer<MongoDbIdentityUserToken> UserTokenComparer = new MongoDbIdentityUserTokenComparer();

        protected static IEqualityComparer<MongoDbIdentityClaim> IdentityClaimComparer = new MongoDbIdentityClaimComparer();

        protected IServiceProvider ServiceProvider;
        private IdentityMongoDbContext _identityDbContext;
        private readonly IUserStore<MongoDbIdentityUser> _userStore;
        private readonly IRoleStore<MongoDbIdentityRole> _roleStore;

        protected MongoDbIdentityStoreTestBase([UsedImplicitly] MongoDbFixture mongoDbFixture)
        {
            ServiceProvider = new ServiceCollection()
                .AddDbContext<IdentityMongoDbContext>(options => options
                    .UseMongoDb(
                        connectionString: MongoDbConstants.MongoUrl,
                        mongoDbOptionsAction: optionsBuilder => optionsBuilder.UseDatabase("__test_identities"))
                    .EnableSensitiveDataLogging(true))
                .AddIdentity<MongoDbIdentityUser, MongoDbIdentityRole>()
                .AddEntityFrameworkMongoDbStores<IdentityMongoDbContext>()
                .Services
                .BuildServiceProvider();
            _identityDbContext = ServiceProvider.GetService<IdentityMongoDbContext>();
            _userStore = ServiceProvider.GetRequiredService<IUserStore<MongoDbIdentityUser>>();
            _roleStore = ServiceProvider.GetRequiredService<IRoleStore<MongoDbIdentityRole>>();
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