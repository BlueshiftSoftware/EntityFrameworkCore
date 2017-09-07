using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using MongoDB.Bson;

namespace Blueshift.EntityFrameworkCore.MongoDB.Storage
{
    /// <summary>
    /// An implementation of <see cref="ITypeMapper" /> that supports MongoDB's <see cref="ObjectId"/>.
    /// </summary>
    public class MongoDbTypeMapper : ITypeMapper
    {
        /// <summary>
        /// Gets a value indicating whether the given .NET type is mapped.
        /// </summary>
        /// <param name="clrType">The .NET type.</param>
        /// <returns>Always returns <c>true</c>, since MongoDB supports arbitrary property types and subdocument structures.</returns>
        public bool IsTypeMapped([NotNull] Type clrType)
            => true;
    }
}
