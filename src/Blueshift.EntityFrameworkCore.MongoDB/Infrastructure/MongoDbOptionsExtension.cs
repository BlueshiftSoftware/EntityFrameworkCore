using System;
using System.Linq;
using System.Text;
using Blueshift.EntityFrameworkCore.MongoDB.DependencyInjection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

// ReSharper disable once CheckNamespace
namespace Blueshift.EntityFrameworkCore.MongoDB.Infrastructure
{
    /// <summary>
    /// This API supports the Entity Framework Core infrastructure and is not intended to be used
    /// directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class MongoDbOptionsExtension : IDbContextOptionsExtension
    {
        private Func<MongoClientSettings, IMongoClient> _mongoClientFactory;
        private MongoClientSettings _mongoClientSettings;
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
                _mongoClientFactory = existing.MongoClientFactory;
                _mongoClientSettings = existing.MongoClientSettings;
                _databaseName = existing.DatabaseName;
                IsQueryLoggingEnabled = existing.IsQueryLoggingEnabled;
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
                => MongoUrl = MongoUrl.Create(Check.NotEmpty(value, nameof(ConnectionString)));
        }

        /// <summary>
        /// This API supports the Entity Framework Core infrastructure and is not intended to be used
        /// directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Func<MongoClientSettings, IMongoClient> MongoClientFactory
        {
            get => _mongoClientFactory;
            [param: NotNull] set
                => _mongoClientFactory = Check.NotNull(value, nameof(MongoClientFactory));
        }

        /// <summary>
        /// This API supports the Entity Framework Core infrastructure and is not intended to be used
        /// directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MongoClientSettings MongoClientSettings
        {
            get => _mongoClientSettings;
            [param: NotNull] set
                => _mongoClientSettings = Check.NotNull(value, nameof(MongoClientSettings)).Clone();
        }

        /// <summary>
        /// This API supports the Entity Framework Core infrastructure and is not intended to be used
        /// directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual MongoUrl MongoUrl
        {
            get => _mongoClientSettings == null
                ? null
                : MongoUrl.Create($"mongodb://{string.Join(",", _mongoClientSettings.Servers.Select(server => $"{server.Host}:{server.Port}"))}");
            [param: NotNull] set
                => MongoClientSettings = MongoClientSettings.FromUrl(Check.NotNull(value, nameof(MongoUrl)));
        }

        /// <summary>
        /// Gets or sets the name of the database that the <see cref="DbContext"/> being configured should use.
        /// </summary>
        public string DatabaseName
        {
            get => _databaseName;
            [param: NotNull] set => _databaseName = Check.NotEmpty(value, nameof(value));
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not logging of queries is enabled.
        /// </summary>
        public bool IsQueryLoggingEnabled { get; set; }

        /// <inheritdoc/>
        public string LogFragment
        {
            get
            {
                if (_logFragment == null)
                {
                    var logBuilder = new StringBuilder();

                    if (_mongoClientSettings != null)
                    {
                        logBuilder.Append($"MongoClient.Settings={_mongoClientSettings}");
                    }

                    if (!string.IsNullOrEmpty(_databaseName))
                    {
                        logBuilder.Append($"DatabaseName={_databaseName}");
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