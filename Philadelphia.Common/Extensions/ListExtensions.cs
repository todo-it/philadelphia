using System.Collections.Generic;

namespace Philadelphia.Common {
    public static class ListExtensions {
        public static void AddRange<T>(this List<T> self, params T[] items) {
            IEnumerable<T> asEnumer = items;
            self.AddRange(asEnumer);
        }

        public static void AddIfTrue<T>(this List<T> self, bool shouldAdd, T elem) {
            if (!shouldAdd) {
                return;
            }
            self.Add(elem);
        }

        public static void AddRangeIfTrue<T>(this List<T> self, bool shouldAdd, params T[] elems) {
            if (!shouldAdd) {
                return;
            }
            self.AddRange(elems);
        }
    }
}
