using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.MongoDB.Annotations
{
    /// <summary>
    /// When applied to a <see cref="DbContext"/>, sets the database name to use with the context's <see cref="Model"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class MongoDatabaseAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoDatabaseAttribute"/> class.
        /// </summary>
        /// <param name="database">The MongoDb database name to use with the <see cref="Model"/>.</param>
        public MongoDatabaseAttribute([NotNull] string database)
        {
            Database = Check.NotEmpty(database, nameof(database));
        }

        /// <summary>
        /// The MongoDb database name to use with the <see cref="Model"/>.
        /// </summary>
        public virtual string Database { get; }
    }
}