using Microsoft.EntityFrameworkCore;

namespace Blueshift.Identity.MongoDB
{
    /// <summary>
    /// A representation of an external user login provider token for use with a MongoDB EntityFramework provider.
    /// </summary>
    [Owned]
    public class MongoDbIdentityUserToken
    {
        /// <summary>
        /// Gets or sets the name of the token.
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Gets or sets the token value.
        /// </summary>
        public virtual string Value { get; set; }
    }
}