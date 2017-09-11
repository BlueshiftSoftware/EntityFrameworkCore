using System.ComponentModel.DataAnnotations;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Bson.Serialization;

namespace Blueshift.EntityFrameworkCore.MongoDB.Adapter
{
    /// <summary>
    /// A convention that sets the <see cref="BsonClassMap.IdMemberMap"/> of a <see cref="BsonClassMap"/>
    /// if that property has been decorated with a <see cref="KeyAttribute"/>.
    /// </summary>
    public class KeyAttributeConvention : BsonMemberMapAttributeConvention<KeyAttribute>
    {
        /// <summary>
        /// Applies the Key Attribute convention to the given <paramref name="memberMap"/>.
        /// </summary>
        /// <param name="memberMap">The <see cref="BsonMemberMap" /> to which the convention will be applied.</param>
        /// <param name="attribute">The <see cref="KeyAttribute"/> to apply.</param>
        protected override void Apply(BsonMemberMap memberMap, KeyAttribute attribute)
        {
            Check.NotNull(memberMap, nameof(memberMap));
            if (memberMap.MemberInfo.IsDefined(typeof(KeyAttribute)))
            {
                memberMap.ClassMap.SetIdMember(memberMap);
            }
        }
    }
}