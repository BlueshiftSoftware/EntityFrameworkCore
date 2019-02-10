using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.Identity.MongoDB
{
    /// <summary>
    /// A representation of an external user login provider for use with a MongoDB EntityFramework provider.
    /// </summary>
    [Owned]
    public class MongoDbIdentityUserLogin : MongoDbIdentityUserLogin<MongoDbIdentityUserToken>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDbIdentityUserLogin"/> class.
        /// </summary>
        public MongoDbIdentityUserLogin() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDbIdentityUserLogin"/> class.
        /// </summary>
        /// <param name="userLoginInfo">The <see cref="UserLoginInfo"/> to use to initialize this user login.</param>
        public MongoDbIdentityUserLogin(UserLoginInfo userLoginInfo)
            : base(userLoginInfo)
        {
        }
    }

    /// <summary>
    /// A representation of an external user login provider for use with a MongoDB EntityFramework provider.
    /// </summary>
    /// <typeparam name="TUserToken">The type of tokens assigned to this external login provider information.</typeparam>
    [Owned]
    public class MongoDbIdentityUserLogin<TUserToken>
        where TUserToken : MongoDbIdentityUserToken
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDbIdentityUserLogin"/> class.
        /// </summary>
        public MongoDbIdentityUserLogin() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDbIdentityUserLogin"/> class.
        /// </summary>
        /// <param name="userLoginInfo">The <see cref="UserLoginInfo"/> to use to initialize this user login.</param>
        public MongoDbIdentityUserLogin(UserLoginInfo userLoginInfo)
        {
            InitializeFromUserLoginInfo(userLoginInfo);
        }

        /// <summary>
        /// Gets or sets the external login provider, such as "Facebook" or "Google"..
        /// </summary>
        public virtual string LoginProvider { get; set; }

        /// <summary>
        /// Gets or sets a unique provider identifier for this login.
        /// </summary>
        public virtual string ProviderKey { get; set; }

        /// <summary>
        /// Gets or sets a friendly name for this login that can be displayed to a user.
        /// </summary>
        public virtual string ProviderDisplayName { get; set; }

        /// <summary>
        /// A collection of <typeparamref name="TUserToken"/> assigned to this provider.
        /// </summary>
        public ICollection<TUserToken> UserTokens { get; [UsedImplicitly] private set; } = new List<TUserToken>();

        /// <summary>
        /// Constructs a new claim with the type and value.
        /// </summary>
        /// <returns>A <see cref="UserLoginInfo"/> that represents this <see cref="MongoDbIdentityUserLogin{TUserToken}"/>.</returns>
        public virtual UserLoginInfo ToUserLoginInfo()
            => new UserLoginInfo(LoginProvider, ProviderKey, ProviderDisplayName);

        /// <summary>
        /// Initializes this <see cref="MongoDbIdentityUserLogin{TUserToken}"/> with values from the given <see cref="UserLoginInfo"/>.
        /// </summary>
        /// <param name="userLoginInfo">The source <see cref="UserLoginInfo"/> to use for initialization.</param>
        public virtual void InitializeFromUserLoginInfo(UserLoginInfo userLoginInfo)
        {
            Check.NotNull(userLoginInfo, nameof(userLoginInfo));
            LoginProvider = userLoginInfo.LoginProvider;
            ProviderKey = userLoginInfo.ProviderKey;
            ProviderDisplayName = userLoginInfo.ProviderDisplayName;
        }
    }
}