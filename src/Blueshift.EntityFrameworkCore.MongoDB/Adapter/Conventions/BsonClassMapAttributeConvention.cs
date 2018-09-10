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
    /// <inheritdoc cref="ConventionBase" />
    /// <inheritdoc cref="IClassMapConvention" />
    /// <summary>
    /// Base class for attribute-based <see cref="T:MongoDB.Bson.Serialization.BsonMemberMap" /> convention processing.
    /// </summary>
    /// <typeparam name="TAttribute">The type of attribute to process.</typeparam>
    public abstract class BsonClassMapAttributeConvention<TAttribute> : ConventionBase, IClassMapConvention
        where TAttribute : Attribute
    {
        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="BsonClassMapAttributeConvention{TAttribute}"/>.
        /// </summary>
        protected BsonClassMapAttributeConvention()
            : base(Regex.Replace(typeof(TAttribute).Name, "Attribute$", ""))
        {
        }


        /// <inheritdoc />
        /// <summary>
        /// Processes each <typeparamref name="TAttribute"/> defined on the given <paramref name="classMap"/>
        /// member info and 
        /// </summary>
        /// <param name="classMap">The <see cref="BsonMemberMap"/> to </param>
        public virtual void Apply(BsonClassMap classMap)
        {
            Check.NotNull(classMap, nameof(classMap));
            IEnumerable<TAttribute> memberMapAttributes = classMap
                .ClassType
                .GetCustomAttributes<TAttribute>();
            foreach (TAttribute attribute in memberMapAttributes)
            {
                Apply(classMap, attribute);
            }
        }

        /// <summary>
        /// Process the conventions on <paramref name="classMap"/> according to the given <paramref name="attribute"/>.
        /// </summary>
        /// <param name="classMap">The <see cref="BsonClassMap"/> to which the conventions will be assigned.</param>
        /// <param name="attribute">The <typeparamref name="TAttribute" /> that defines the convention.</param>
        protected abstract void Apply([NotNull] BsonClassMap classMap, [NotNull] TAttribute attribute);
    }
}