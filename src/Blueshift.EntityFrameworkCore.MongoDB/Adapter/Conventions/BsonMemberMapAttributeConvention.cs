using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;

namespace Blueshift.EntityFrameworkCore.MongoDB.Adapter.Conventions
{
    /// <summary>
    /// Base class for attribute-based <see cref="BsonMemberMap"/> convention processing.
    /// </summary>
    /// <typeparam name="TAttribute">The type of attribute to process.</typeparam>
    public abstract class BsonMemberMapAttributeConvention<TAttribute> : ConventionBase, IMemberMapConvention
        where TAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BsonMemberMapAttributeConvention{TAttribute}"/>.
        /// </summary>
        protected BsonMemberMapAttributeConvention()
            : base(Regex.Replace(typeof(TAttribute).Name, "Attribute$", ""))
        {
        }

        /// <summary>
        /// Processes each <typeparamref name="TAttribute"/> defined on the given <paramref name="memberMap"/>
        /// member info and 
        /// </summary>
        /// <param name="memberMap">The <see cref="BsonMemberMap"/> to </param>
        public virtual void Apply([NotNull] BsonMemberMap memberMap)
        {
            Check.NotNull(memberMap, nameof(memberMap));
            IEnumerable<TAttribute> memberMapAttributes = memberMap.MemberInfo
                .GetCustomAttributes<TAttribute>();
            foreach (TAttribute attribute in memberMapAttributes)
            {
                Apply(memberMap, attribute);
            }
        }

        /// <summary>
        /// Process the conventions on <paramref name="memberMap"/> according to the given <paramref name="attribute"/>.
        /// </summary>
        /// <param name="memberMap">The <see cref="BsonMemberMap"/> to which the conventions will be assigned.</param>
        /// <param name="attribute">The <typeparamref name="TAttribute" /> that defines the convention.</param>
        protected abstract void Apply([NotNull] BsonMemberMap memberMap, [NotNull] TAttribute attribute);
    }
}