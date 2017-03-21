using System;
using Blueshift.EntityFrameworkCore.MongoDB.Adapter;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace Blueshift.EntityFrameworkCore.Infrastructure
{
    public class MongoDbOptionsExtension : IDbContextOptionsExtension
    {
        private string _connectionString;
        private MongoClientSettings _mongoClientSettings;
        private MongoUrl _mongoUrl;
        private IMongoClient _mongoClient;

        public MongoDbOptionsExtension([CanBeNull]MongoDbOptionsExtension existing = null)
        {
            if (existing != null)
            {
                CopyOptions(existing);
            }
        }

        private void CopyOptions(MongoDbOptionsExtension existing)
        {
            _connectionString = existing.ConnectionString;
            _mongoClient = existing.MongoClient;
            _mongoUrl = existing.MongoUrl;
            _mongoClient = existing.MongoClient;
        }

        public virtual string ConnectionString
        {
            get { return _connectionString; }
            [param: NotNull] set
            {
                _connectionString = Check.NotEmpty(value, nameof(ConnectionString));
            }
        }

        public virtual IMongoClient MongoClient
        {
            get { return _mongoClient; }
            [param: NotNull] set
            {
                _mongoClient = Check.NotNull(value, nameof(MongoClient));
            }
        }

        public virtual MongoClientSettings MongoClientSettings
        {
            get { return _mongoClientSettings; }
            [param: NotNull] set
            {
                _mongoClientSettings = Check.NotNull(value, nameof(MongoClientSettings)).Clone();
            }
        }

        public virtual MongoUrl MongoUrl
        {
            get { return _mongoUrl; }
            [param: NotNull] set
            {
                _mongoUrl = Check.NotNull(value, nameof(MongoUrl));
            }
        }

        public virtual bool ApplyServices([NotNull] IServiceCollection services)
        {
            ConventionRegistry.Register(
                name: "EntityFramework.MongoDb.Conventions",
                conventions: EntityFrameworkConventionPack.Instance,
                filter: type => true);
            Check.NotNull(services, nameof(services)).AddEntityFrameworkMongoDb();
            return true;
        }

        public virtual long GetServiceProviderHashCode() => 0;

        public virtual void Validate(IDbContextOptions options)
        {
        }
    }
}