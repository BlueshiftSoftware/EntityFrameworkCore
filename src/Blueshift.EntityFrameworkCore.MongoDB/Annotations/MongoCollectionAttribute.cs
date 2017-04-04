using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Blueshift.EntityFrameworkCore.MongoDB.Annotations
{
    /// <summary>
    /// When applied to an entity class, sets the name of MongoDB collection name used to store instances of the entity.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class MongoCollectionAttribute : Attribute, IEntityTypeConvention
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

        /// <summary>
        /// This API supports the Entity Framework Core infrastructure and is not intended to be used directly from
        /// your code. This API may change or be removed in future releases.
        /// </summary>
        public InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder))
                .MongoDb()
                .CollectionName = CollectionName;
            return entityTypeBuilder;
        }
    }
}