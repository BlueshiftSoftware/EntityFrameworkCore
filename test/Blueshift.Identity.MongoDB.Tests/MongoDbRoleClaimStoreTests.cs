using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
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

        private IRoleClaimStore<MongoDbIdentityRole> _mongoDbRoleClaimStore;

        public MongoDbRoleClaimStoreTests(MongoDbIdentityFixture mongoDbIdentityFixture)
            : base(mongoDbIdentityFixture)
        {
            _mongoDbRoleClaimStore = Services.GetRequiredService<IRoleClaimStore<MongoDbIdentityRole>>();
        }

        protected static IEqualityComparer<Claim> ClaimComparer
            => new FuncEqualityComparer<Claim>((claim1, claim2)
                => string.Equals(claim1.Type, claim2.Type, StringComparison.Ordinal)
                    && string.Equals(claim1.Value, claim2.Value, StringComparison.Ordinal));

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
            await _mongoDbRoleClaimStore.AddClaimAsync(role, Claim3, new CancellationToken());
            var newClaims = new Claim[] { Claim1, Claim2, Claim3 };
            Assert.Equal(newClaims, role.Claims.Select(claim => claim.ToClaim()), ClaimComparer);
        }

        [Fact]
        public async void Can_get_claims_async()
        {
            var role = CreateRole();
            var claims = new Claim[] { Claim1, Claim2 };
            Assert.Equal(claims, await _mongoDbRoleClaimStore.GetClaimsAsync(role, new CancellationToken()), ClaimComparer);
        }

        [Fact]
        public async void Can_remove_claims_async()
        {
            var role = CreateRole();
            var claimsToRemove = new Claim[] { Claim1, Claim2 };
            await _mongoDbRoleClaimStore.RemoveClaimAsync(role, Claim1, new CancellationToken());
            Assert.Equal(Claim2, role.Claims.Select(claim => claim.ToClaim()).Single(), ClaimComparer);
        }
    }
}