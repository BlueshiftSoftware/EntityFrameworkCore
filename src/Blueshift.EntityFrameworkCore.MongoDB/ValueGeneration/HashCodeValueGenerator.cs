using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using MongoDB.Bson;

namespace Blueshift.EntityFrameworkCore.MongoDB.ValueGeneration
{
    /// <inheritdoc />
    public class HashCodeValueGenerator : ValueGenerator<int?>
    {
        /// <inheritdoc />
        /// <summary>
        ///     Generates a new <see cref="ObjectId"/> value.
        /// </summary>
        /// <param name="entry">The <see cref="EntityEntry"/> whose value is to be generated.</param>
        /// <returns>A new <see cref="ObjectId"/> for <see cref="EntityEntry"/>.</returns>
        public override int? Next(EntityEntry entry)
            => entry.Entity?.GetHashCode();

        /// <inheritdoc />
        /// <summary>
        ///     <code>true</code> if this <see cref="HashCodeValueGenerator"/> generates temporary values;
        ///     otherwise <code>false</code>.
        /// </summary>
        /// <remarks>Always returns <c>true</c>.</remarks>
        public override bool GeneratesTemporaryValues => true;
    }
}