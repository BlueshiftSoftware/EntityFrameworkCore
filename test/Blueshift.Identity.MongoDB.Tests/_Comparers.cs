using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Blueshift.Identity.MongoDB;

namespace Blueshift.Identity.MongoDB.Tests
{
    public class ClaimComparer : EqualityComparer<Claim>
    {
        public override bool Equals(Claim claim1, Claim claim2)
            => (claim1 != null && claim2 != null)
               || string.Equals(claim1.Type, claim2.Type, StringComparison.Ordinal)
               && string.Equals(claim1.Value, claim2.Value, StringComparison.Ordinal);

        public override int GetHashCode(Claim obj)
            => obj?.GetHashCode() ?? throw new ArgumentNullException(nameof(obj));
    }

    public class MongoDbIdentityRoleComparer : EqualityComparer<MongoDbIdentityRole>
    {
        public override bool Equals(MongoDbIdentityRole role1, MongoDbIdentityRole role2)
            => (role1 != null && role2 != null)
                || role1.Id == role2.Id
                && string.Equals(role1.RoleName, role2.RoleName, StringComparison.Ordinal)
                && string.Equals(role1.NormalizedRoleName, role2.NormalizedRoleName, StringComparison.Ordinal)
                && role1.Claims.Count == role2.Claims.Count
                && role1.Claims.All(item => role2.Claims.Contains(item, new MongoDbIdentityClaimComparer()))
                && role1.Claims.All(item => role2.Claims.Contains(item, new MongoDbIdentityClaimComparer()));

        public override int GetHashCode(MongoDbIdentityRole obj)
            => obj?.GetHashCode() ?? throw new ArgumentNullException(nameof(obj));
    }

    public class MongoDbIdentityUserComparer : EqualityComparer<MongoDbIdentityUser>
    {
        public override bool Equals(MongoDbIdentityUser user1, MongoDbIdentityUser user2)
            => (user1 != null && user2 != null)
                || user1.Id == user2.Id
                && string.Equals(user1.UserName, user2.UserName, StringComparison.Ordinal)
                && string.Equals(user1.NormalizedUserName, user2.NormalizedUserName, StringComparison.Ordinal)
                && user1.Logins.Count == user2.Logins.Count
                && user1.Logins.All(item => user2.Logins.Contains(item, new MongoDbIdentityUserLoginComparer()))
                && user1.Logins.All(item => user2.Logins.Contains(item, new MongoDbIdentityUserLoginComparer()))
                && user1.Claims.Count == user2.Claims.Count
                && user1.Claims.All(item => user2.Claims.Contains(item, new MongoDbIdentityClaimComparer()))
                && user1.Claims.All(item => user2.Claims.Contains(item, new MongoDbIdentityClaimComparer()));

        public override int GetHashCode(MongoDbIdentityUser obj)
            => obj?.GetHashCode() ?? throw new ArgumentNullException(nameof(obj));
    }

    public class MongoDbIdentityUserLoginComparer : EqualityComparer<MongoDbIdentityUserLogin>
    {
        public override bool Equals(MongoDbIdentityUserLogin userLogin1, MongoDbIdentityUserLogin userLogin2)
            => (userLogin1 != null && userLogin2 != null)
                || string.Equals(userLogin1.LoginProvider, userLogin2.LoginProvider, StringComparison.Ordinal)
                && string.Equals(userLogin1.ProviderKey, userLogin2.ProviderKey, StringComparison.Ordinal)
                && string.Equals(userLogin1.ProviderDisplayName, userLogin2.ProviderDisplayName, StringComparison.Ordinal)
                && userLogin1.UserTokens.Count == userLogin2.UserTokens.Count
                && userLogin1.UserTokens.All(item => userLogin2.UserTokens.Contains(item, new MongoDbIdentityUserTokenComparer()))
                && userLogin1.UserTokens.All(item => userLogin2.UserTokens.Contains(item, new MongoDbIdentityUserTokenComparer()));

        public override int GetHashCode(MongoDbIdentityUserLogin obj)
            => obj?.GetHashCode() ?? throw new ArgumentNullException(nameof(obj));
    }

    public class MongoDbIdentityUserTokenComparer : EqualityComparer<MongoDbIdentityUserToken>
    {
        public override bool Equals(MongoDbIdentityUserToken userToken1, MongoDbIdentityUserToken userToken2)
            => (userToken1 != null && userToken2 != null)
                || string.Equals(userToken1.Name, userToken2.Name, StringComparison.Ordinal)
                && string.Equals(userToken1.Value, userToken2.Value, StringComparison.Ordinal);

        public override int GetHashCode(MongoDbIdentityUserToken obj)
            => obj?.GetHashCode() ?? throw new ArgumentNullException(nameof(obj));
    }

    public class MongoDbIdentityClaimComparer : EqualityComparer<MongoDbIdentityClaim>
    {
        public override bool Equals(MongoDbIdentityClaim claim1, MongoDbIdentityClaim claim2)
            => (claim1 != null && claim2 != null)
                || string.Equals(claim1.ClaimType, claim2.ClaimType, StringComparison.Ordinal)
                && string.Equals(claim1.ClaimValue, claim2.ClaimValue, StringComparison.Ordinal);

        public override int GetHashCode(MongoDbIdentityClaim obj)
            => obj?.GetHashCode() ?? throw new ArgumentNullException(nameof(obj));
    }
}
