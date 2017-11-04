using System.Linq;
using System.Security.Claims;
using Blueshift.MongoDB.Tests.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Blueshift.Identity.MongoDB.Tests
{
    public class MongoDbRoleClaimStoreTests : MongoDbIdentityStoreTestBase
    {
        private const string Claim1Type = nameof(Claim1Type);
        private const string Claim1Value = nameof(Claim1Value);
        private const string Claim2Type = nameof(Claim2Type);
        private const string Claim2Value = nameof(Claim2Value);
        private const string Claim3Type = nameof(Claim3Type);
        private const string Claim3Value = nameof(Claim3Value);

        private static readonly Claim Claim1 = new Claim(Claim1Type, Claim1Value);
        private static readonly Claim Claim2 = new Claim(Claim2Type, Claim2Value);
        private static readonly Claim Claim3 = new Claim(Claim3Type, Claim3Value);

        private readonly IRoleClaimStore<MongoDbIdentityRole> _mongoDbRoleClaimStore;

        public MongoDbRoleClaimStoreTests(MongoDbFixture mongoDbFixture)
            : base(mongoDbFixture)
        {
            _mongoDbRoleClaimStore = ServiceProvider.GetRequiredService<IRoleClaimStore<MongoDbIdentityRole>>();
        }

        protected override MongoDbIdentityRole CreateRole()
        {
            var role = base.CreateRole();
            role.Claims.Add(new MongoDbIdentityClaim(Claim1));
            role.Claims.Add(new MongoDbIdentityClaim(Claim2));
            return role;
        }

        [Fact]
        public async void Can_add_claim_async()
        {
            var role = CreateRole();
            await _mongoDbRoleClaimStore.AddClaimAsync(role, Claim3);
            var newClaims = new [] { Claim1, Claim2, Claim3 };
            Assert.Equal(newClaims, role.Claims.Select(claim => claim.ToClaim()), new ClaimComparer());
        }

        [Fact]
        public async void Can_get_claims_async()
        {
            var role = CreateRole();
            var claims = new [] { Claim1, Claim2 };
            Assert.Equal(claims, await _mongoDbRoleClaimStore.GetClaimsAsync(role), new ClaimComparer());
        }

        [Fact]
        public async void Can_remove_claims_async()
        {
            var role = CreateRole();
            await _mongoDbRoleClaimStore.RemoveClaimAsync(role, Claim1);
            Assert.Equal(Claim2, role.Claims.Select(claim => claim.ToClaim()).Single(), new ClaimComparer());
        }
    }
}