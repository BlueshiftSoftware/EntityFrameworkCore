using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;

namespace Blueshift.EntityFrameworkCore.MongoDB.Adapter
{
    /// <summary>
    /// A convention that sets the <see cref="BsonClassMap.IdMemberMap"/> of a <see cref="BsonClassMap"/>
    /// if that property has been decorated with a <see cref="KeyAttribute"/>.
    /// </summary>
    public class KeyAttributeConvention : ConventionBase, IMemberMapConvention
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyAttributeConvention"/> class.
        /// </summary>
        public KeyAttributeConvention()
            : base(Regex.Replace(nameof(KeyAttributeConvention), pattern: "Convention$", replacement: ""))
        {
        }

        /// <summary>
        /// Applies the Key Attribute convention to the given <paramref name="memberMap"/>.
        /// </summary>
        /// <param name="memberMap">The <see cref="BsonMemberMap" /> to which the convention will be applied.</param>
        public virtual void Apply([NotNull] BsonMemberMap memberMap)
        {
            Check.NotNull(memberMap, nameof(memberMap));
            if (memberMap.MemberInfo.IsDefined(typeof(KeyAttribute)))
            {
                memberMap.ClassMap.SetIdMember(memberMap);
            }
        }
    }
}