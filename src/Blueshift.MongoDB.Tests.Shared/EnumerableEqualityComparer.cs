using System.Collections.Generic;
using System.Linq;

namespace Blueshift.MongoDB.Tests.Shared
{
    public class EnumerableEqualityComparer<T> : IEqualityComparer<IEnumerable<T>>
    {
        public static bool Equals(IEnumerable<T> list1, IEnumerable<T> list2, IEqualityComparer<T> comparer)
            => new EnumerableEqualityComparer<T>(comparer).Equals(list1, list2);

        private readonly IEqualityComparer<T> _comparer;

        public EnumerableEqualityComparer(IEqualityComparer<T> comparer)
        {
            _comparer = comparer ?? EqualityComparer<T>.Default;
        }

        public bool Equals(IEnumerable<T> list1, IEnumerable<T> list2)
            => (list1 == null && list2 == null)
                || ((list1.Count() == list2.Count())
                    && list1.All(item => list2.Contains(item, _comparer))
                    && list2.All(item => list1.Contains(item, _comparer)));

        public int GetHashCode(IEnumerable<T> obj)
            => obj?.GetHashCode() ?? 0;
    }
}