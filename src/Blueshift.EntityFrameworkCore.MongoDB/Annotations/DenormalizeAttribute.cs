using System;

namespace Blueshift.EntityFrameworkCore.MongoDB.Annotations
{
    /// <summary>
    /// Declares that a member of a navigation property should be denormalized when serializing the property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class DenormalizeAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DenormalizeAttribute"/> class.
        /// </summary>
        /// <param name="memberNames">The names of sub-document members to denormalize when serializing the parent document.</param>
        public DenormalizeAttribute(params string[] memberNames)
        {
            MemberNames = memberNames ?? new string[0];
        }

        /// <summary>
        /// The name of the member to denormalize.
        /// </summary>
        public string[] MemberNames { get; }
    }
}