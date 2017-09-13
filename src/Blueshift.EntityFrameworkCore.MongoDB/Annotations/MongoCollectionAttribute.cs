using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.MongoDB.Annotations
{
    /// <summary>
    /// When applied to an entity class, sets the name of MongoDB collection name used to store instances of the entity.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class MongoCollectionAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoCollectionAttribute"/> class.
        /// </summary>
        /// <param name="collectionName">The MongoDb database name to use with the <see cref="Model"/>.</param>
        public MongoCollectionAttribute([NotNull] string collectionName)
        {
            CollectionName = Check.NotEmpty(collectionName, nameof(collectionName));
        }

        /// <summary>
        /// The MongoDb database name to use with the <see cref="Model"/>.
        /// </summary>
        public virtual string CollectionName { get; }
    }
}
