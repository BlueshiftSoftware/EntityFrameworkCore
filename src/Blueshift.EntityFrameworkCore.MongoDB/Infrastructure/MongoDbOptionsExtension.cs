using Blueshift.EntityFrameworkCore.MongoDB.Adapter;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    /// This API supports the Entity Framework Core infrastructure and is not intended to be used
    /// directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class MongoDbOptionsExtension : IDbContextOptionsExtension
    {
        private IMongoClient _mongoClient;
        private string _databaseName;

        /// <summary>
        /// This API supports the Entity Framework Core infrastructure and is not intended to be used
        /// directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public MongoDbOptionsExtension([CanBeNull]MongoDbOptionsExtension existing = null)
        {
            _mongoClient = existing?.MongoClient;
        }

        /// <summary>
        /// This API supports the Entity Framework Core infrastructure and is not intended to be used
        /// directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string ConnectionString
        {
            get { return MongoUrl?.ToString(); }
            [param: NotNull] set
            {
                MongoUrl = MongoUrl.Create(Check.NotEmpty(value, nameof(ConnectionString)));
            }
        }

        /// <summary>
        /// This API supports the Entity Framework Core infrastructure and is not intended to be used
        /// directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IMongoClient MongoClient
        {
            get { return _mongoClient; }
            [param: NotNull] set
            {
                _mongoClient = Check.NotNull(value, nameof(MongoClient));
            }
        }

        /// <summary>
        /// This API supports the Entity Framework Core infrastructure and is not intended to be used
        /// directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MongoClientSettings MongoClientSettings
        {
            get { return MongoClient?.Settings; }
            [param: NotNull] set
            {
                MongoClient = new MongoClient(Check.NotNull(value, nameof(MongoClientSettings)).Clone());
            }
        }

        /// <summary>
        /// This API supports the Entity Framework Core infrastructure and is not intended to be used
        /// directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MongoUrl MongoUrl
        {
            get { return MongoUrl.Create(MongoClientSettings.ToString()); }
            [param: NotNull] set
            {
                MongoClientSettings = MongoClientSettings.FromUrl(Check.NotNull(value, nameof(MongoUrl)));
            }
        }

        /// <summary>
        /// Gets or sets the name of the database that the <see cref="DbContext"/> being configured should use.
        /// </summary>
        public string DatabaseName
        {
            get { return _databaseName; }
            [param: NotNull] set
            {
                _databaseName = Check.NotEmpty(value, nameof(value));
            }
        }

        /// <summary>
        /// This API supports the Entity Framework Core infrastructure and is not intended to be used
        /// directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool ApplyServices([NotNull] IServiceCollection services)
        {
            ConventionRegistry.Register(
                name: "EntityFramework.MongoDb.Conventions",
                conventions: EntityFrameworkConventionPack.Instance,
                filter: type => true);
            Check.NotNull(services, nameof(services)).AddEntityFrameworkMongoDb();
            return true;
        }

        /// <summary>
        /// This API supports the Entity Framework Core infrastructure and is not intended to be used
        /// directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual long GetServiceProviderHashCode() => 0;

        /// <summary>
        /// This API supports the Entity Framework Core infrastructure and is not intended to be used
        /// directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Validate(IDbContextOptions options)
        {
        }
    }
}