using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace System.Collections.Generic
{
    /// <summary>
    /// Provides a set of extensions for <see cref="IList{T}"/>.
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// Replaces instances of <typeparamref name="TBase"/> in <paramref name="replacement"/> with the
        /// given <paramref name="replacement"/>.
        /// </summary>
        /// <typeparam name="TBase">The base type of instances to be replaced.</typeparam>
        /// <typeparam name="TReplacement">The type of the replacement.</typeparam>
        /// <param name="list">An <see cref="IList{TBase}"/> whose values will be replaced.</param>
        /// <param name="replacement">The new value to be inserted in <paramref name="list"/>.</param>
        /// <returns>The modified <paramref name="list"/>, such that calls can be chained.</returns>
        public static IList<TBase> Replace<TBase, TReplacement>([NotNull] this IList<TBase> list,
            [NotNull] TReplacement replacement)
            where TReplacement : TBase
        {
            Check.NotNull(list, nameof(list));
            Check.NotNull(replacement, nameof(replacement));
            list
                .OfType<TReplacement>()
                .Select(item => list.IndexOf(item))
                .ToList()
                .ForEach(index => list[index] = replacement);
            return list;
        }

        /// <summary>
        /// Adds an <paramref name="item"/> to the given <paramref name="list"/>.
        /// </summary>
        /// <typeparam name="T">The type of items contained in <paramref name="list"/>.</typeparam>
        /// <param name="list">The <see cref="IList{T}"/> to which <paramref name="item"/> will be added.</param>
        /// <param name="item">The instance of <typeparamref name="T"/> to add to <paramref name="list"/>.</param>
        /// <returns>The modified <paramref name="list"/>, such that calls can be chained.</returns>
        public static IList<T> With<T>([NotNull] this IList<T> list,
            [NotNull] T item)
        {
            Check.NotNull(list, nameof(list));
            Check.NotNull(item, nameof(item));
            list.Add(item);
            return list;
        }
    }
}