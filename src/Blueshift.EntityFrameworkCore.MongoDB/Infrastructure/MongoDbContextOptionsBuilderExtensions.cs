using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Driver;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// MongoDb-specific extension methods for <see cref="DbContextOptionsBuilder" />.
    /// </summary>
    public static class MongoDbContextOptionsBuilderExtensions
    {
        /// <summary>
        ///     Configures the context to connect to a MongoDb instance.
        /// </summary>
        /// <param name="optionsBuilder">The builder being used to configure the context.</param>
        /// <param name="connectionString">The connection string of the MongoDb instance to connect to.</param>
        /// <param name="mongoDbOptionsAction">An optional action to allow additional MongoDb-specific configuration.</param>
        /// <returns> The options builder so that further configuration can be chained. </returns>
        public static DbContextOptionsBuilder UseMongoDb(
            [NotNull] this DbContextOptionsBuilder optionsBuilder,
            [NotNull] string connectionString,
            [CanBeNull] Action<MongoDbContextOptionsBuilder> mongoDbOptionsAction = null)
        {
            Check.NotEmpty(connectionString, nameof(connectionString));
            return SetupMongoDb(Check.NotNull(optionsBuilder, nameof(optionsBuilder)),
                extension => extension.ConnectionString = connectionString,
                mongoDbOptionsAction);
        }

        /// <summary>
        ///     Configures the context to connect to a MongoDb instance.
        /// </summary>
        /// <param name="optionsBuilder">The builder being used to configure the context.</param>
        /// <param name="mongoClient">The <see cref="IMongoClient"/> to use when connecting to MongoDb.</param>
        /// <param name="mongoDbOptionsAction">An optional action to allow additional MongoDb-specific configuration.</param>
        /// <returns> The options builder so that further configuration can be chained. </returns>
        public static DbContextOptionsBuilder UseMongoDb(
            [NotNull] this DbContextOptionsBuilder optionsBuilder,
            [NotNull] IMongoClient mongoClient,
            [CanBeNull] Action<MongoDbContextOptionsBuilder> mongoDbOptionsAction = null)
        {
            Check.NotNull(mongoClient, nameof(mongoClient));
            return SetupMongoDb(Check.NotNull(optionsBuilder, nameof(optionsBuilder)),
                extension => extension.MongoClient = mongoClient,
                mongoDbOptionsAction);
        }

        /// <summary>
        ///     Configures the context to connect to a MongoDb instance.
        /// </summary>
        /// <param name="optionsBuilder">The builder being used to configure the context.</param>
        /// <param name="mongoClientSettings">The <see cref="MongoClientSettings"/> to use when connecting to MongoDb.</param>
        /// <param name="mongoDbOptionsAction">An optional action to allow additional MongoDb-specific configuration.</param>
        /// <returns> The options builder so that further configuration can be chained. </returns>
        public static DbContextOptionsBuilder UseMongoDb(
            [NotNull] this DbContextOptionsBuilder optionsBuilder,
            [NotNull] MongoClientSettings mongoClientSettings,
            [CanBeNull] Action<MongoDbContextOptionsBuilder> mongoDbOptionsAction = null)
        {
            Check.NotNull(mongoClientSettings, nameof(mongoClientSettings));
            return SetupMongoDb(Check.NotNull(optionsBuilder, nameof(optionsBuilder)),
                extension => extension.MongoClientSettings = mongoClientSettings,
                mongoDbOptionsAction);
        }

        /// <summary>
        ///     Configures the context to connect to a MongoDb instance.
        /// </summary>
        /// <param name="optionsBuilder">The builder being used to configure the context.</param>
        /// <param name="mongoUrl">The <see cref="MongoUrl"/> to use to connect to MongoDb.</param>
        /// <param name="mongoDbOptionsAction">An optional action to allow additional MongoDb-specific configuration.</param>
        /// <returns> The options builder so that further configuration can be chained. </returns>
        public static DbContextOptionsBuilder UseMongoDb(
            [NotNull] this DbContextOptionsBuilder optionsBuilder,
            [NotNull] MongoUrl mongoUrl,
            [CanBeNull] Action<MongoDbContextOptionsBuilder> mongoDbOptionsAction = null)
        {
            Check.NotNull(mongoUrl, nameof(mongoUrl));
            return SetupMongoDb(Check.NotNull(optionsBuilder, nameof(optionsBuilder)),
                extension => extension.MongoUrl = mongoUrl,
                mongoDbOptionsAction);
        }

        private static DbContextOptionsBuilder SetupMongoDb(
            [NotNull] DbContextOptionsBuilder optionsBuilder,
            [NotNull] Action<MongoDbOptionsExtension> mongoDbOptionsExtensionAction,
            [CanBeNull] Action<MongoDbContextOptionsBuilder> mongoDbOptionsAction)
        {
            MongoDbOptionsExtension extension = GetOrCreateExtension(optionsBuilder);
            mongoDbOptionsExtensionAction(extension);
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

            ConfigureWarnings(optionsBuilder);

            mongoDbOptionsAction?.Invoke(new MongoDbContextOptionsBuilder(optionsBuilder));

            return optionsBuilder;
        }

        private static MongoDbOptionsExtension GetOrCreateExtension([NotNull] DbContextOptionsBuilder optionsBuilder)
        {
            var existing = optionsBuilder.Options.FindExtension<MongoDbOptionsExtension>();
            return existing != null
                ? new MongoDbOptionsExtension(existing)
                : new MongoDbOptionsExtension();
        }

        private static void ConfigureWarnings([NotNull] DbContextOptionsBuilder optionsBuilder)
            => Check.NotNull(optionsBuilder, nameof(optionsBuilder))
                .ConfigureWarnings(warningsConfigurationBuilder =>
                {
                    warningsConfigurationBuilder.Default(WarningBehavior.Log);
                });
    }
}