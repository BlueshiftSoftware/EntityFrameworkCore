using System;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Blueshift.EntityFrameworkCore.MongoDB.Storage
{
    /// <summary>
    /// An implementation of <see cref="ITypeMapper" /> that supports MongoDB's <see cref="ObjectId"/>.
    /// </summary>
    public class MongoDbTypeMapper : ITypeMapper
    {
        private readonly ConcurrentDictionary<Type, bool> _typeCache = new ConcurrentDictionary<Type, bool>();

        /// <summary>
        /// Gets a value indicating whether the given .NET type is mapped.
        /// </summary>
        /// <param name="clrType">The .NET type.</param>
        /// <returns>Always returns <c>true</c>, since MongoDB supports arbitrary property types and subdocument structures.</returns>
        public bool IsTypeMapped(Type clrType)
            => _typeCache.GetOrAdd(
                Check.NotNull(clrType, nameof(clrType)),
                type => type.IsPrimitive()
                    || type == typeof(ObjectId)
                    || !type.GetProperties()
                        .Any(property => property.IsDefined(typeof(KeyAttribute), true)
                                         || property.IsDefined(typeof(BsonIdAttribute), true)));
    }
}
