using System;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Blueshift.EntityFrameworkCore
{
    /// <summary>
    /// A set of utilities to assist with MongoDb values.
    /// </summary>
    public static class MongoDbUtilities
    {
        private static readonly Regex LeadingUppercaseRegex
            = new Regex(pattern: "^(([A-Z](?![a-z]))+|([A-Z](?=[a-z])))", options: RegexOptions.Compiled);
        private static readonly Regex SingularRegex
            = new Regex(pattern: "(ey|.)(?<!s)$", options: RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Converts the first character or group of capital characters of a string to lower camel case.
        /// </summary>
        /// <param name="value">The string to convert to lower camel case.</param>
        /// <returns>A string containing a lower camel case version of the original <paramref name="value"/>.</returns>
        public static string ToLowerCamelCase([NotNull] string value)
            => LeadingUppercaseRegex.Replace(
                Check.NotNull(value, nameof(value)), match => match.Value.ToLower());

        /// <summary>
        /// Pluralizes a string by replacing any trailing 'y' with 'ies'.
        /// </summary>
        /// <param name="value">The string to pluralize.</param>
        /// <returns>A pluralized version of the original <paramref name="value"/>.</returns>
        public static string Pluralize([NotNull] string value)
            => SingularRegex.Replace(
                Check.NotNull(value, nameof(value)),
                match => string.Equals(a: "y", b: match.Value, comparisonType: StringComparison.OrdinalIgnoreCase)
                    ? "ies"
                    : $"{match.Value}s");
    }
}