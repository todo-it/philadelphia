using System.Collections.Generic;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public static class IterableExtensions {
        public static IEnumerable<T> AsIEnumerable<T>(this IIterable<T> self) {
            return new IterableIEnumerator<T>(self).AsEnumerable();
        }
    }
}
