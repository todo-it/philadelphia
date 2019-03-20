using System;
using System.Collections.Generic;
using System.Linq;

namespace Philadelphia.Common {
    public static class SetExtensions {
        public static void AddRange<T>(this ISet<T> self, IEnumerable<T> items) {
            items.ForEach(x => self.Add(x));
        }

        public static void IfTrueAdd<T>(this ISet<T> self, bool isTrue, T toAdd) {
            if (isTrue) {
                self.Add(toAdd);
            }
        }

        public static void IfEmptyTry<T>(this ISet<T> self, Func<bool> hasError, T itm) {
            if (self.Any()) {
                return;
            }

            if (hasError()) {
                self.Add(itm);
            }
        }

        public static void Replace<T>(this ISet<T> self, IEnumerable<T> newValue) {
            self.Clear();
            self.AddRange(newValue);
        }
    }
}
