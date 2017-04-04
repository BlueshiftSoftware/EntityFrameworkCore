using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Blueshift.Identity.MongoDB
{
    /// <summary>
    /// A representation of an authorization role for use with a MongoDB EntityFramework provider.
    /// </summary>
    public class MongoDbIdentityRole
        : MongoDbIdentityRole<ObjectId>
    {
    }

    /// <summary>
    /// A representation of an authorization role for use with a MongoDB EntityFramework provider.
    /// </summary>
    /// <typeparam name="TKey">The type of the role's identifier.</typeparam>
    public class MongoDbIdentityRole<TKey>
        : MongoDbIdentityRole<TKey, MongoDbIdentityClaim>
        where TKey : IEquatable<TKey>
    {
    }

    /// <summary>
    /// A representation of an authorization role for use with a MongoDB EntityFramework provider.
    /// </summary>
    /// <typeparam name="TKey">The type of the role's identifier.</typeparam>
    /// <typeparam name="TClaim">The type of authorization claims assigned to this role.</typeparam>
    public class MongoDbIdentityRole<TKey, TClaim>
        where TKey : IEquatable<TKey>
        where TClaim : MongoDbIdentityClaim
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDbIdentityRole{TKey, TClaim}"/> class.
        /// </summary>
        public MongoDbIdentityRole() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDbIdentityRole{TKey, TClaim}"/> class.
        /// </summary>
        /// <param name="roleName">The name of the role.</param>
        public MongoDbIdentityRole(string roleName)
        {
            RoleName = Check.NotNull(roleName, nameof(roleName));
        }

        /// <summary>
        /// A collection of the security claims assigned to this role.
        /// </summary>
        public virtual ICollection<TClaim> Claims { get; private set; } = new List<TClaim>();

        /// <summary>
        /// Gets or sets the primary key for this role.
        /// </summary>
        [BsonId]
        public virtual TKey Id { get; private set; }

        /// <summary>
        /// Gets or sets the name for this role.
        /// </summary>
        public virtual string RoleName { get; set; }

        /// <summary>
        /// Gets or sets the normalized name for this role.
        /// </summary>
        public virtual string NormalizedRoleName { get; set; }

        /// <summary>
        /// A random value that should change whenever a role is persisted to the store.
        /// </summary>
        public virtual string ConcurrencyStamp { get; set; }

        /// <summary>
        /// Returns the name of this <see cref="MongoDbIdentityRole{TKey, TClaim}"/>.
        /// </summary>
        /// <returns>The name of this role.</returns>
        public override string ToString() => RoleName;
    }
}