using System;
using Blueshift.EntityFrameworkCore.MongoDB.Annotations;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;

namespace Blueshift.Identity.MongoDB
{
    /// <inheritdoc />
    /// <summary>
    /// Base class for the Entity Framework MongoDB database context used for identity.
    /// </summary>
    [MongoDatabase("__identities")]
    public class IdentityMongoDbContext
        : IdentityMongoDbContext<MongoDbIdentityUser, MongoDbIdentityRole>
    {
        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of <see cref="T:Blueshift.Identity.MongoDB.IdentityMongoDbContext" />.
        /// </summary>
        /// <param name="options">The options to be used by a <see cref="T:Microsoft.EntityFrameworkCore.DbContext" />.</param>
        public IdentityMongoDbContext(DbContextOptions<IdentityMongoDbContext> options)
            : base(options)
        { }

        /// <summary>
        /// Initializes a new instance of <see cref="IdentityMongoDbContext"/>.
        /// </summary>
        /// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
        protected IdentityMongoDbContext(DbContextOptions options)
            : base(options) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityMongoDbContext" /> class.
        /// </summary>
        protected IdentityMongoDbContext() { }
    }

    /// <summary>
    /// Base class for the Entity Framework MongoDB database context used for identity.
    /// </summary>
    /// <typeparam name="TKey">The type of the primary key for users and roles.</typeparam>
    [MongoDatabase("__identities")]
    public class IdentityMongoDbContext<TKey>
        : IdentityMongoDbContext<
            MongoDbIdentityUser<TKey>,
            MongoDbIdentityRole<TKey>,
            TKey,
            MongoDbIdentityClaim,
            MongoDbIdentityUserRole,
            MongoDbIdentityUserLogin,
            MongoDbIdentityUserToken>
        where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="IdentityMongoDbContext{TKey}"/>.
        /// </summary>
        /// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
        public IdentityMongoDbContext(DbContextOptions<IdentityMongoDbContext<TKey>> options)
            : base(options)
        { }

        /// <summary>
        /// Initializes a new instance of <see cref="IdentityMongoDbContext{TKey}"/>.
        /// </summary>
        /// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
        protected IdentityMongoDbContext(DbContextOptions options)
            : base(options) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityMongoDbContext{TKey}" /> class.
        /// </summary>
        protected IdentityMongoDbContext() { }
    }

    /// <inheritdoc />
    /// <summary>
    /// Base class for the Entity Framework MongoDB database context used for identity.
    /// </summary>
    /// <typeparam name="TUser">The type of user objects.</typeparam>
    /// <typeparam name="TRole">The type of role objects.</typeparam>
    [MongoDatabase("__identities")]
    public class IdentityMongoDbContext<TUser, TRole>
        : IdentityMongoDbContext<TUser, TRole, ObjectId>
        where TUser : MongoDbIdentityUser<ObjectId>
        where TRole : MongoDbIdentityRole<ObjectId>
    {
        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of <see cref="T:Blueshift.Identity.MongoDB.IdentityMongoDbContext`2" />.
        /// </summary>
        /// <param name="options">The options to be used by a <see cref="T:Microsoft.EntityFrameworkCore.DbContext" />.</param>
        public IdentityMongoDbContext(DbContextOptions<IdentityMongoDbContext<TUser, TRole>> options) 
            : base(options)
        { }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of <see cref="T:Blueshift.Identity.MongoDB.IdentityMongoDbContext`2" />.
        /// </summary>
        /// <param name="options">The options to be used by a <see cref="T:Microsoft.EntityFrameworkCore.DbContext" />.</param>
        protected IdentityMongoDbContext(DbContextOptions options)
            : base(options) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityMongoDbContext{TUser, TRole}" /> class.
        /// </summary>
        protected IdentityMongoDbContext() { }
    }

    /// <inheritdoc />
    /// <summary>
    /// Base class for the Entity Framework MongoDB database context used for identity.
    /// </summary>
    /// <typeparam name="TUser">The type of user objects.</typeparam>
    /// <typeparam name="TRole">The type of role objects.</typeparam>
    /// <typeparam name="TKey">The type of the primary key for users and roles.</typeparam>
    [MongoDatabase("__identities")]
    public class IdentityMongoDbContext<TUser, TRole, TKey>
        : IdentityMongoDbContext<
            TUser,
            TRole,
            TKey,
            MongoDbIdentityClaim,
            MongoDbIdentityUserRole,
            MongoDbIdentityUserLogin,
            MongoDbIdentityUserToken>
        where TUser : MongoDbIdentityUser<
            TKey,
            MongoDbIdentityClaim,
            MongoDbIdentityUserRole,
            MongoDbIdentityUserLogin,
            MongoDbIdentityUserToken>
        where TRole : MongoDbIdentityRole<
            TKey,
            MongoDbIdentityClaim>
        where TKey : IEquatable<TKey>
    {
        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of <see cref="T:Blueshift.Identity.MongoDB.IdentityMongoDbContext`3" />.
        /// </summary>
        /// <param name="options">The options to be used by a <see cref="T:Microsoft.EntityFrameworkCore.DbContext" />.</param>
        public IdentityMongoDbContext(DbContextOptions<IdentityMongoDbContext<TUser, TRole, TKey>> options)
            : base(options)
        { }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of <see cref="T:Blueshift.Identity.MongoDB.IdentityMongoDbContext`3" />.
        /// </summary>
        /// <param name="options">The options to be used by a <see cref="T:Microsoft.EntityFrameworkCore.DbContext" />.</param>
        protected IdentityMongoDbContext(DbContextOptions options)
            : base(options) { }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Blueshift.Identity.MongoDB.IdentityMongoDbContext`3" /> class.
        /// </summary>
        protected IdentityMongoDbContext() { }
    }

    /// <inheritdoc />
    /// <summary>
    /// Base class for the Entity Framework MongoDB database context used for identity.
    /// </summary>
    /// <typeparam name="TUser">The type of user objects.</typeparam>
    /// <typeparam name="TRole">The type of role objects.</typeparam>
    /// <typeparam name="TKey">The type of the primary key for users and roles.</typeparam>
    /// <typeparam name="TClaim">The type of the role claim object.</typeparam>
    /// <typeparam name="TUserRole">The type of the user role object.</typeparam>
    /// <typeparam name="TUserLogin">The type of the user login object.</typeparam>
    /// <typeparam name="TUserToken">The type of the user token object.</typeparam>
    [MongoDatabase("__identities")]
    public class IdentityMongoDbContext<TUser, TRole, TKey, TClaim, TUserRole, TUserLogin, TUserToken> : DbContext
        where TUser : MongoDbIdentityUser<TKey, TClaim, TUserRole, TUserLogin, TUserToken>
        where TRole : MongoDbIdentityRole<TKey, TClaim>
        where TKey : IEquatable<TKey>
        where TClaim : MongoDbIdentityClaim
        where TUserRole : MongoDbIdentityUserRole
        where TUserLogin : MongoDbIdentityUserLogin<TUserToken>
        where TUserToken : MongoDbIdentityUserToken
    {
        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of <see cref="T:Blueshift.Identity.MongoDB.IdentityMongoDbContext`7" />.
        /// </summary>
        /// <param name="options">The options to be used by a <see cref="T:Microsoft.EntityFrameworkCore.DbContext" />.</param>
        public IdentityMongoDbContext(DbContextOptions<IdentityMongoDbContext<TUser, TRole, TKey, TClaim, TUserRole, TUserLogin, TUserToken>> options)
            : base(options) { }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of <see cref="T:Blueshift.Identity.MongoDB.IdentityMongoDbContext`7" />.
        /// </summary>
        /// <param name="options">The options to be used by a <see cref="T:Microsoft.EntityFrameworkCore.DbContext" />.</param>
        protected IdentityMongoDbContext(DbContextOptions options)
            : base(options) { }

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Blueshift.Identity.MongoDB.IdentityMongoDbContext`7" /> class.
        /// </summary>
        protected IdentityMongoDbContext() { }

        /// <summary>
        /// Gets or sets the <see cref="DbSet{TUser}"/> used to store user documents.
        /// </summary>
        public DbSet<TUser> Users { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DbSet{TEntity}"/> of roles.
        /// </summary>
        public DbSet<TRole> Roles { get; set; }
    }
}