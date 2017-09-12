using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Bson.Serialization;

namespace Blueshift.EntityFrameworkCore.MongoDB.Adapter.Conventions
{
    /// <summary>
    /// Marks a <see cref="BsonMemberMap"/> as ignored during serialization.
    /// </summary>
    public class NotMappedAttributeConvention : BsonMemberMapAttributeConvention<NotMappedAttribute>
    {
        /// <summary>
        /// Applies the Not Mapped convention to the given <paramref name="memberMap"/>.
        /// </summary>
        /// <param name="memberMap">The <see cref="BsonMemberMap" /> to which the convention will be applied.</param>
        /// <param name="attribute">The <see cref="NotMappedAttribute"/> to apply.</param>
        protected override void Apply(BsonMemberMap memberMap, NotMappedAttribute attribute)
            => Check.NotNull(memberMap, nameof(memberMap))
                .ClassMap
                .UnmapMember(memberMap.MemberInfo);
    }
}