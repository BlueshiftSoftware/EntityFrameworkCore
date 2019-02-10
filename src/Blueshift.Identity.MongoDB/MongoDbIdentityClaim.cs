using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.Identity.MongoDB
{
    /// <summary>
    /// A representation of an authorization claim for use with a MongoDB EntityFramework provider.
    /// </summary>
    [Owned]
    public class MongoDbIdentityClaim
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDbIdentityClaim"/> class.
        /// </summary>
        public MongoDbIdentityClaim() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDbIdentityClaim"/> class.
        /// </summary>
        /// <param name="claim">The security <see cref="Claim"/> to use to initialize this role claim.</param>
        public MongoDbIdentityClaim(Claim claim)
        {
            InitializeFromClaim(claim);
        }

        /// <summary>
        /// Gets or sets the claim type for this claim.
        /// </summary>
        public virtual string ClaimType { get; set; }

        /// <summary>
        /// Gets or sets the claim value for this claim.
        /// </summary>
        public virtual string ClaimValue { get; set; }

        /// <summary>
        /// Constructs a new claim with the type and value.
        /// </summary>
        /// <returns>A <see cref="Claim"/> that represents this <see cref="MongoDbIdentityClaim"/>.</returns>
        public virtual Claim ToClaim()
            => new Claim(ClaimType, ClaimValue);

        /// <summary>
        /// Initializes this <see cref="MongoDbIdentityClaim"/> with values from the given <see cref="Claim"/>.
        /// </summary>
        /// <param name="claim">The source <see cref="Claim"/> to use for initialization.</param>
        public virtual void InitializeFromClaim(Claim claim)
        {
            Check.NotNull(claim, nameof(claim));
            ClaimType = claim.Type;
            ClaimValue = claim.Value;
        }
    }
}