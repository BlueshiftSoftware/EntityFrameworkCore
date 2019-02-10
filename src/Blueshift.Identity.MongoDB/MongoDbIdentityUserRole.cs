using Microsoft.EntityFrameworkCore;

namespace Blueshift.Identity.MongoDB
{
    /// <summary>
    /// A representation of a user's security authorization role for use with a MongoDB EntityFramework provider.
    /// </summary>
    [Owned]
    public class MongoDbIdentityUserRole
    {
        /// <summary>
        /// Gets or sets the name of the role that the user is in.
        /// </summary>
        public string RoleName { get; set; }

        /// <summary>
        /// Gets or sets the normalized name of the role that the user is in.
        /// </summary>
        public string NormalizedRoleName { get; set; }
    }
}