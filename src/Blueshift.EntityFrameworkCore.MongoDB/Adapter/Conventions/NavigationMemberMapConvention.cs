using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Blueshift.EntityFrameworkCore.MongoDB.Annotations;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;

namespace Blueshift.EntityFrameworkCore.MongoDB.Adapter.Conventions
{
    /// <summary>
    /// A convention for specifying how to serialize navigation properties.
    /// </summary>
    public class NavigationMemberMapConvention : ConventionBase, IMemberMapConvention
    {
        /// <inheritdoc />
        public void Apply([NotNull] BsonMemberMap bsonMemberMap)
        {
            Type memberType = Check.NotNull(bsonMemberMap, nameof(bsonMemberMap)).MemberType;
            Type sequenceType = memberType.TryGetSequenceType();
            BsonClassMap memberClassMap = BsonClassMap.LookupClassMap(sequenceType ?? memberType);
            if (memberClassMap.IdMemberMap != null)
            {
                IEnumerable<BsonMemberMap> denormalizedProperties = bsonMemberMap.MemberInfo
                    .GetCustomAttribute<DenormalizeAttribute>()
                    ?.MemberNames
                    .Select(memberName => memberClassMap.GetMemberMap(memberName));
                IBsonSerializer bsonSerializer = BsonSerializer.LookupSerializer(memberType)
                    .AsNavigationBsonSerializer(denormalizedProperties ?? new BsonMemberMap[0]);
                bsonMemberMap.SetSerializer(bsonSerializer);
            }
        }
    }
}
