using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Blueshift.EntityFrameworkCore.MongoDB.Adapter.Serialization;
using Blueshift.EntityFrameworkCore.MongoDB.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Conventions;

namespace Blueshift.EntityFrameworkCore.MongoDB.Adapter.Conventions
{
    /// <inheritdoc cref="ConventionBase" />
    /// <inheritdoc cref="IPostProcessingConvention" />
    /// <summary>
    /// A convention for specifying how to serialize navigation properties.
    /// </summary>
    public class NavigationSrializationMemberMapConvention : ConventionBase, IMemberMapConvention
    {
        /// <inheritdoc />
        /// <summary>
        /// Checks whether the member map represents a navigation, and sets the member map's serializer.
        /// </summary>
        /// <param name="memberMap">The member map.</param>
        public void Apply(BsonMemberMap memberMap)
        {
            Type memberTargetType = Check.NotNull(memberMap, nameof(memberMap)).MemberType.TryGetSequenceType()
                                    ?? memberMap.MemberType;
            if (!memberTargetType.IsPrimitive && HasIdMember(memberTargetType))
            {
                IBsonSerializer memberMapSerializer = (IBsonSerializer) Activator.CreateInstance(
                    typeof(NavigationBsonMemberMapSerializer<>).MakeGenericType(memberTargetType),
                    memberMap);
                memberMap.SetSerializer(memberMapSerializer);
            }
        }

        private bool HasIdMember(Type type)
            => !type.IsPrimitive
               && type
                   .GetMembers(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance)
                   .Any(memberInfo => memberInfo.IsDefined(typeof(BsonIdAttribute))
                                      || memberInfo.IsDefined(typeof(KeyAttribute)));
    }
}
