using System;
using System.Collections.Generic;

namespace Philadelphia.Web {
    public class CompareImpl<T> : IComparer<T> {
        private readonly Func<T,T,int> _impl;

        public CompareImpl(Func<T,T,int> impl) {
            _impl = impl;
        }

        public int Compare(T x, T y) {
            return _impl(x, y);
        }
    }
}
