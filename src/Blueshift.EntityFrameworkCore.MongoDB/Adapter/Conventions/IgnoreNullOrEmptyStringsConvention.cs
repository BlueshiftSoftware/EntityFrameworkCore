using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;

namespace Blueshift.EntityFrameworkCore.MongoDB.Adapter.Conventions
{
    /// <summary>
    /// Instructs the MongoDb C# driver to ignore null, empty, or default values of <see cref="string"/> properties.
    /// </summary>
    public class IgnoreNullOrEmptyStringsConvention : ConventionBase, IMemberMapConvention
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IgnoreNullOrEmptyStringsConvention"/> class.
        /// </summary>
        public IgnoreNullOrEmptyStringsConvention()
            : base(Regex.Replace(nameof(IgnoreNullOrEmptyStringsConvention), "Convention$", ""))
        {
        }

        /// <summary>
        /// Applies the Ignore Null or Empty Strings convention to the given <paramref name="memberMap"/>.
        /// </summary>
        /// <param name="memberMap">The <see cref="BsonMemberMap" /> to which the convention will be applied.</param>
        public virtual void Apply([NotNull] BsonMemberMap memberMap)
        {
            Check.NotNull(memberMap, nameof(memberMap));
            if (memberMap.MemberType == typeof(string))
            {
                SetShouldSerializeMethod(memberMap);
            }
        }

        private static void SetShouldSerializeMethod(BsonMemberMap memberMap)
        {
            var defaultString = memberMap.DefaultValue as string;
            if (!string.IsNullOrEmpty(defaultString))
            {
                ShouldSerializeIfNotDefault(memberMap, defaultString);
            }
            else
            {
                ShouldSerializeIfNotEmpty(memberMap);
            }
        }

        private static void ShouldSerializeIfNotEmpty(BsonMemberMap memberMap)
            => memberMap.SetShouldSerializeMethod(@object => !string.IsNullOrEmpty(memberMap.Getter(@object) as string));

        private static void ShouldSerializeIfNotDefault(BsonMemberMap memberMap, string defaultString)
            => memberMap.SetShouldSerializeMethod(@object => !string.Equals(defaultString, memberMap.Getter(@object) as string));
    }
}