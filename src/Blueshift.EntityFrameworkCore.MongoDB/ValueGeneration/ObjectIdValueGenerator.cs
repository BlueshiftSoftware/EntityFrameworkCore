using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using MongoDB.Bson;

namespace Blueshift.EntityFrameworkCore.MongoDB.ValueGeneration
{
    /// <summary>
    ///     Generates values for <see cref="ObjectId"/> properties when an entity is added to a context.
    /// </summary>
    public class ObjectIdValueGenerator : ValueGenerator<ObjectId>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ObjectIdValueGenerator"/> class.
        /// </summary>
        /// <param name="generatesTemporaryValue">
        ///     <code>true</code> if this <see cref="ObjectIdValueGenerator"/> generates temporary values;
        ///     otherwise <code>false</code>.
        /// </param>
        public ObjectIdValueGenerator(bool generatesTemporaryValue = false)
        {
            GeneratesTemporaryValues = generatesTemporaryValue;
        }

        /// <summary>
        ///     Generates a new <see cref="ObjectId"/> value.
        /// </summary>
        /// <param name="entry">The <see cref="EntityEntry"/> whose value is to be generated.</param>
        /// <returns>A new <see cref="ObjectId"/> for <see cref="EntityEntry"/>.</returns>
        public override ObjectId Next(EntityEntry entry)
            => ObjectId.GenerateNewId();

        /// <summary>
        ///     <code>true</code> if this <see cref="ObjectIdValueGenerator"/> generates temporary values;
        ///     otherwise <code>false</code>.
        /// </summary>
        public override bool GeneratesTemporaryValues { get; }
    }
}