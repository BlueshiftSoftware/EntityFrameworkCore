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
        /// Inserts the given <paramref name="item"/> before the first instance of <typeparamref name="TExisting"/>.
        /// </summary>
        /// <typeparam name="T">The type of items contained in the list.</typeparam>
        /// <typeparam name="TExisting">The type of the existing item.</typeparam>
        /// <param name="list">An <see cref="IList{TBase}"/> whose values will be replaced.</param>
        /// <param name="item">The new value to be inserted in <paramref name="list"/>.</param>
        /// <returns>The modified <paramref name="list"/>, such that calls can be chained.</returns>
        public static IList<T> InsertBefore<T, TExisting>([NotNull] this IList<T> list,
            [NotNull] T item)
            where TExisting : T
        {
            Check.NotNull(list, nameof(list));
            Check.NotNull(item, nameof(item));
            TExisting existing = list.OfType<TExisting>().FirstOrDefault();
            int index = existing != null
                ? list.IndexOf(existing)
                : list.Count;
            list.Insert(index, item);
            return list;
        }

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

        /// <summary>
        /// Removes all items that match <paramref name="predicate"/> from the given <paramref name="list"/>.
        /// </summary>
        /// <typeparam name="T">The type of items contained in <paramref name="list"/>.</typeparam>
        /// <param name="list">The <see cref="IList{T}"/> from which items will be removed.</param>
        /// <param name="predicate">A <see cref="Func{T1,T2}"/> to test for elements to remove.</param>
        /// <returns>The modified <paramref name="list"/>, such that calls can be chained.</returns>
        public static IList<T> Without<T>([NotNull] this IList<T> list, Func<T, bool> predicate)
        {
            Check.NotNull(list, nameof(list))
                .Where(predicate)
                .ToList()
                .ForEach(item => list.Remove(item));
            return list;
        }
    }
}