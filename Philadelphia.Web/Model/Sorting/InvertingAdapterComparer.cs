using System.Collections.Generic;

namespace Philadelphia.Web {
    public class InvertingAdapterComparer<T> : IComparer<T> {
        private readonly IComparer<T> _original;

        public InvertingAdapterComparer(IComparer<T> original) {
            _original = original;
        }

        public int Compare(T x, T y) {
            return -_original.Compare(x, y);
        }
    }
}
