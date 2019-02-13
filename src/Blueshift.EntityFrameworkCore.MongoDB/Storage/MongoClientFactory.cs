using Blueshift.EntityFrameworkCore.MongoDB.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;

namespace Blueshift.EntityFrameworkCore.MongoDB.Storage
{
    /// <inheritdoc />
    public class MongoClientFactory : IMongoClientFactory
    {
        [NotNull] private readonly IDiagnosticsLogger<DbLoggerCategory.Database.Command> _commandDiagnosticsLogger;
        [NotNull] private readonly MongoClientSettings _mongoClientSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoClientFactory"/> class.
        /// </summary>
        /// <param name="mongoClientSettings">The <see cref="MongoClientSettings"/> used to configure the <see cref="IMongoClient"/>
        /// instances created by this factory.</param>
        /// <param name="commandDiagnosticsLogger">A <see cref="IDiagnosticsLogger{TDbLoggerCategory}"/> used to log
        /// commands executed by the <see cref="IMongoClient"/> instances created by this factory.</param>
        public MongoClientFactory(
            [NotNull] MongoClientSettings mongoClientSettings,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Database.Command> commandDiagnosticsLogger)
        {
            _mongoClientSettings = Check.NotNull(mongoClientSettings, nameof(mongoClientSettings));
            _commandDiagnosticsLogger = Check.NotNull(commandDiagnosticsLogger, nameof(commandDiagnosticsLogger));

            mongoClientSettings.ClusterConfigurator = clusterBuilder =>
                clusterBuilder.Subscribe<CommandStartedEvent>(LogCommandExecution);
        }

        /// <inheritdoc />
        public IMongoClient CreateMongoClient()
            => new MongoClient(_mongoClientSettings.Clone());

        private void LogCommandExecution(CommandStartedEvent commandStartedEvent)
        {
            if (commandStartedEvent.CommandName == "aggregate")
            {
                _commandDiagnosticsLogger.DocumentQueryCommand(commandStartedEvent.Command.ToJson());
            }
        }
    }
}