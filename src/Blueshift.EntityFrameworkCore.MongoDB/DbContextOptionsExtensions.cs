using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Blueshift.EntityFrameworkCore
{
    /// <summary>
    /// Provides a set of extension methods for instances of the <see cref="IDbContextOptions"/> interface.
    /// </summary>
    public static class DbContextOptionsExtensions
    {
        /// <summary>
        /// Extracts a single instance of the <typeparamref name="TExtension"/> from this <paramref name="dbContextOptions"/>.
        /// </summary>
        /// <typeparam name="TExtension">The type of <see cref="IDbContextOptionsExtension"/> to be extracted.</typeparam>
        /// <param name="dbContextOptions">An instance of <see cref="IDbContextOptions"/> to search for the given extension.</param>
        /// <returns>The single instance of <typeparamref name="TExtension"/> that was found.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if no instance of the <typeparamref name="TExtension"/> could be found, or if more than one was found.
        /// </exception>
        public static TExtension Extract<TExtension>([NotNull] this IDbContextOptions dbContextOptions)
            where TExtension : IDbContextOptionsExtension
        {
            Check.NotNull(dbContextOptions, nameof(dbContextOptions));

            IList<TExtension> extensions = dbContextOptions.Extensions
                .OfType<TExtension>()
                .ToList();

            if (extensions.Count == 0)
            {
                throw new InvalidOperationException($"No provider has been configured with a {nameof(TExtension)} extension.");
            }
            if (extensions.Count > 1)
            {
                throw new InvalidOperationException($"Multiple providers have been configured with a {nameof(TExtension)} extension.");
            }
            return extensions[index: 0];
        }
    }
}