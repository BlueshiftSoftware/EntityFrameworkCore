using System;
using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace Blueshift.EntityFrameworkCore.MongoDB.Storage
{
    /// <summary>
    /// Determines whether a .NET type can be mapped to a MongoDB database type.
    /// </summary>
    public class MongoDbTypeMapper : ITypeMapper
    {
        private readonly ConcurrentDictionary<Type, bool> _typeCache = new ConcurrentDictionary<Type, bool>
        {
            [typeof(string)] = true,
            [typeof(ObjectId)] = true
        };

        /// <summary>
        /// Gets a value indicating whether the given .NET type is mapped.
        /// </summary>
        /// <param name="clrType">The .NET type.</param>
        /// <returns>Always returns <c>true</c>, since MongoDB supports arbitrary property types and subdocument structures.</returns>
        public bool IsTypeMapped(Type clrType)
            => _typeCache.GetOrAdd(
                Check.NotNull(clrType, nameof(clrType)),
                type =>
                {
                    clrType = (clrType.TryGetSequenceType() ?? clrType).UnwrapNullableType();
                    TypeInfo typeInfo = clrType.GetTypeInfo();
                    return typeInfo.IsPrimitive
                        || typeInfo.IsValueType
                        || (typeInfo.IsClass && BsonClassMap.LookupClassMap(clrType).IdMemberMap == null);
                });
    }
}
