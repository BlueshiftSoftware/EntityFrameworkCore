using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Bson;
using MongoDB.Driver.Core.Events;

// ReSharper disable once CheckNamespace
namespace Blueshift.EntityFrameworkCore.MongoDB.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         Allows MongoDb-specific configuration to be performed on <see cref="DbContextOptions" />.
    ///     </para>
    ///     <para>
    ///         Instances of this class are returned from a call to
    ///         <see cref="MongoDbContextOptionsBuilderExtensions.UseMongoDb(DbContextOptionsBuilder, string, System.Action{MongoDbContextOptionsBuilder})" />
    ///         and it is not designed to be directly constructed in your application code.
    ///     </para>
    /// </summary>
    public class MongoDbContextOptionsBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDbContextOptionsBuilder"/> class.
        /// </summary>
        /// <param name="optionsBuilder">The core <see cref="DbContextOptionsBuilder"/> class.</param>
        public MongoDbContextOptionsBuilder([NotNull] DbContextOptionsBuilder optionsBuilder)
        {
            OptionsBuilder = Check.NotNull(optionsBuilder, nameof(optionsBuilder));
        }

        /// <summary>
        /// Gets the core <see cref="DbContextOptionsBuilder"/> that supplied to the constructor.
        /// </summary>
        protected virtual DbContextOptionsBuilder OptionsBuilder { get; }

        /// <summary>
        /// Sets the name of the MongoDB database to use with the <see cref="DbContext"/> being configured.
        /// </summary>
        /// <param name="databaseName">The name of the MongoDB database instance to use with the current <see cref="DbContext"/>.</param>
        /// <returns>This <see cref="MongoDbOptionsExtension"/>, so that calls can be chained.</returns>
        public MongoDbContextOptionsBuilder UseDatabase([NotNull] string databaseName)
        {
            Check.NotEmpty(databaseName, nameof(databaseName));
            MongoDbOptionsExtension extension = CloneExtension();
            extension.DatabaseName = databaseName;
            ((IDbContextOptionsBuilderInfrastructure)OptionsBuilder).AddOrUpdateExtension(extension);
            return this;
        }

        /// <summary>
        ///     Clones the <see cref="MongoDbOptionsExtension"/> used to configure this builder.
        /// </summary>
        /// <returns>A cloned instance of this builder's <see cref="MongoDbOptionsExtension"/>.</returns>
        protected virtual MongoDbOptionsExtension CloneExtension()
            => new MongoDbOptionsExtension(OptionsBuilder.Options.GetExtension<MongoDbOptionsExtension>());

        /// <summary>
        /// Sets the name of the MongoDB database to use with the <see cref="DbContext"/> being configured.
        /// </summary>
        /// <returns>This <see cref="MongoDbOptionsExtension"/>, so that calls can be chained.</returns>
        public MongoDbContextOptionsBuilder EnableQueryLogging()
        {
            MongoDbOptionsExtension extension = CloneExtension();
            extension.IsQueryLoggingEnabled = true;
            ((IDbContextOptionsBuilderInfrastructure)OptionsBuilder).AddOrUpdateExtension(extension);
            return this;
        }
    }
}
