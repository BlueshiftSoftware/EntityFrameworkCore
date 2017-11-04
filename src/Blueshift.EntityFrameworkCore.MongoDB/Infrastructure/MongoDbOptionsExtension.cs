using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
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
        private string _logFragment;

        /// <summary>
        /// This API supports the Entity Framework Core infrastructure and is not intended to be used
        /// directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public MongoDbOptionsExtension([CanBeNull]MongoDbOptionsExtension existing = null)
        {
            if (existing != null)
            {
                _mongoClient = existing.MongoClient;
                _databaseName = existing.DatabaseName;
            }
        }

        /// <summary>
        /// This API supports the Entity Framework Core infrastructure and is not intended to be used
        /// directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string ConnectionString
        {
            get => MongoUrl?.ToString();
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
            get => _mongoClient;
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
            get => MongoClient?.Settings;
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
            get => _mongoClient == null
                ? null
                : MongoUrl.Create($"mongodb://{string.Join(",", _mongoClient.Settings.Servers.Select(server => $"{server.Host}:{server.Port}"))}");
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
            get => _databaseName;
            [param: NotNull] set => _databaseName = Check.NotEmpty(value, nameof(value));
        }

        /// <inheritdoc/>
        public string LogFragment
        {
            get
            {
                if (_logFragment == null)
                {
                    var logBuilder = new StringBuilder();

                    if (_mongoClient?.Settings != null)
                    {
                        logBuilder.Append("MongoClient.Settings=").Append(_mongoClient.Settings.ToString());
                    }

                    if (!string.IsNullOrEmpty(DatabaseName))
                    {
                        logBuilder.Append("DatabaseName=").Append(DatabaseName);
                    }

                    _logFragment = logBuilder.ToString();
                }
                return _logFragment;
            }
        }

        /// <summary>
        /// This API supports the Entity Framework Core infrastructure and is not intended to be used
        /// directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool ApplyServices(IServiceCollection services)
        {
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