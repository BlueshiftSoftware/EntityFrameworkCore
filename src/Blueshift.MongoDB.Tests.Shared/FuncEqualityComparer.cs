using System;
using System.Collections.Generic;

namespace Blueshift.MongoDB.Tests.Shared
{
    public class FuncEqualityComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> _comparer;

        public FuncEqualityComparer(Func<T, T, bool> comparer)
        {
            _comparer = comparer;
        }

        public bool Equals(T x, T y) => 
            (ReferenceEquals(x, default(T)) && ReferenceEquals(y, default(T))) ||
            (!ReferenceEquals(x, y) && !ReferenceEquals(x, default(T)) && !ReferenceEquals(y, default(T)) && _comparer(x, y));

        public int GetHashCode(T obj) => obj.GetHashCode();
    }
}