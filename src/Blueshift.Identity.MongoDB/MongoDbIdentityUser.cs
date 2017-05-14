using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Blueshift.EntityFrameworkCore.MongoDB.Annotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Blueshift.Identity.MongoDB
{
    /// <summary>
    /// A representation of a user security principal for use with a MongoDB EntityFramework provider.
    /// </summary>
    [MongoCollection("users")]
    public class MongoDbIdentityUser : MongoDbIdentityUser<ObjectId>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MongoDbIdentityUser"/>.
        /// </summary>
        public MongoDbIdentityUser() { }

        /// <summary>
        /// Initializes a new instance of <see cref="MongoDbIdentityUser"/>.
        /// </summary>
        /// <param name="userName">The user name.</param>
        public MongoDbIdentityUser(string userName) : base(userName) { }
    }

    /// <summary>
    /// A representation of a user security principal for use with a MongoDB EntityFramework provider.
    /// </summary>
    /// <typeparam name="TKey">The type of the primary key for this object.</typeparam>
    [MongoCollection("users")]
    public class MongoDbIdentityUser<TKey>
        : MongoDbIdentityUser<TKey, MongoDbIdentityClaim, MongoDbIdentityUserRole, MongoDbIdentityUserLogin, MongoDbIdentityUserToken>
        where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MongoDbIdentityUser{TKey}"/>.
        /// </summary>
        public MongoDbIdentityUser() { }

        /// <summary>
        /// Initializes a new instance of <see cref="MongoDbIdentityUser{TKey}"/>.
        /// </summary>
        /// <param name="userName">The user name.</param>
        public MongoDbIdentityUser(string userName) : base(userName) { }
    }

    /// <summary>
    /// A representation of a user security principal for use with a MongoDB EntityFramework provider.
    /// </summary>
    /// <typeparam name="TKey">The type of the primary key for this object.</typeparam>
    /// <typeparam name="TClaim">The type of security claims assigned to this user.</typeparam>
    /// <typeparam name="TUserRole">The type security roles assigned to this user.</typeparam>
    /// <typeparam name="TUserLogin">The type of external login provider information assigned to this user.</typeparam>
    /// <typeparam name="TUserToken">The type of external login provider tokens assigned to this user.</typeparam>
    [MongoCollection("users")]
    public class MongoDbIdentityUser<TKey, TClaim, TUserRole, TUserLogin, TUserToken>
        where TKey : IEquatable<TKey>
        where TClaim : MongoDbIdentityClaim
        where TUserRole : MongoDbIdentityUserRole
        where TUserLogin : MongoDbIdentityUserLogin<TUserToken>
        where TUserToken : MongoDbIdentityUserToken
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MongoDbIdentityUser{TKey, TUserClaim, TUserRole, TUserLogin, TUserToken}"/>.
        /// </summary>
        public MongoDbIdentityUser() { }

        /// <summary>
        /// Initializes a new instance of <see cref="MongoDbIdentityUser{TKey, TUserClaim, TUserRole, TUserLogin, TUserToken}"/>.
        /// </summary>
        /// <param name="userName">The user name.</param>
        public MongoDbIdentityUser(string userName)
            : this()
        {
            UserName = userName;
        }

        /// <summary>
        /// Gets or sets the primary key for this user.
        /// </summary>
        [BsonId, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual TKey Id { get; private set; }

        /// <summary>
        /// Gets or sets the user name for this user.
        /// </summary>
        public virtual string UserName { get; set; }

        /// <summary>
        /// Gets or sets the normalized user name for this user.
        /// </summary>
        public virtual string NormalizedUserName { get; set; }

        /// <summary>
        /// Gets or sets the email address for this user.
        /// </summary>
        public virtual string Email { get; set; }

        /// <summary>
        /// Gets or sets the normalized email address for this user.
        /// </summary>
        public virtual string NormalizedEmail { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if a user has confirmed their email address.
        /// </summary>
        /// <value>True if the email address has been confirmed, otherwise false.</value>
        public virtual bool EmailConfirmed { get; set; }

        /// <summary>
        /// Gets or sets a salted and hashed representation of the password for this user.
        /// </summary>
        public virtual string PasswordHash { get; set; }

        /// <summary>
        /// A random value that must change whenever a users credentials change (password changed, login removed)
        /// </summary>
        public virtual string SecurityStamp { get; set; }

        /// <summary>
        /// A random value that must change whenever a user is persisted to the store
        /// </summary>
        [ConcurrencyCheck]
        public virtual string ConcurrencyStamp { get; set; }

        /// <summary>
        /// Gets or sets a telephone number for the user.
        /// </summary>
        public virtual string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if a user has confirmed their telephone address.
        /// </summary>
        /// <value>True if the telephone number has been confirmed, otherwise false.</value>
        public virtual bool PhoneNumberConfirmed { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if two factor authentication is enabled for this user.
        /// </summary>
        /// <value>True if 2fa is enabled, otherwise false.</value>
        public virtual bool TwoFactorEnabled { get; set; }

        /// <summary>
        /// Gets or sets the date and time, in UTC, when any user lockout ends.
        /// </summary>
        /// <remarks>
        /// A value in the past means the user is not locked out.
        /// </remarks>
        public virtual DateTimeOffset? LockoutEnd { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the user could be locked out.
        /// </summary>
        /// <value>True if the user could be locked out, otherwise false.</value>
        public virtual bool LockoutEnabled { get; set; }

        /// <summary>
        /// Gets or sets the number of failed login attempts for the current user.
        /// </summary>
        public virtual int AccessFailedCount { get; set; }

        /// <summary>
        /// A collection of security roles assigned to this user.
        /// </summary>
        public virtual ICollection<TUserRole> Roles { get; private set; } = new List<TUserRole>();

        /// <summary>
        /// A collection of security claims assigned to this user.
        /// </summary>
        public virtual ICollection<TClaim> Claims { get; private set; } = new List<TClaim>();

        /// <summary>
        /// A collection of external login provider information assigned to this user.
        /// </summary>
        public virtual ICollection<TUserLogin> Logins { get; private set; } = new List<TUserLogin>();

        /// <summary>
        /// Returns the <see cref="UserName"/> for this user.
        /// </summary>
        /// <returns>The username of this user.</returns>
        public override string ToString() => UserName;
    }
}