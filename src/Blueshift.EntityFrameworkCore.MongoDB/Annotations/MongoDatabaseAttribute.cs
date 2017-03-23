using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.MongoDB.Annotations
{
    /// <summary>
    /// When applied to a <see cref="DbContext"/>, sets the database name to use with the context's <see cref="Model"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class MongoDatabaseAttribute : Attribute, IModelConvention
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDatabaseAttribute"/> class.
        /// </summary>
        /// <param name="database">The MongoDb database name to use with the <see cref="Model"/>.</param>
        public MongoDatabaseAttribute([NotNull] string database)
        {
            if (string.IsNullOrWhiteSpace(database))
            {
                throw new ArgumentException(message: "Database name cannot be null, empty, or exclusively white-space.", paramName: nameof(database));
            }
            Database = database;
        }

        /// <summary>
        /// The MongoDb database name to use with the <see cref="Model"/>.
        /// </summary>
        public virtual string Database { get; }

        /// <summary>
        /// This API supports the Entity Framework Core infrastructure and is not intended to be used directly from
        /// your code. This API may change or be removed in future releases.
        /// </summary>
        public InternalModelBuilder Apply(InternalModelBuilder modelBuilder)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder))
                .MongoDb()
                .Database = Database;
            return modelBuilder;
        }
    }
}