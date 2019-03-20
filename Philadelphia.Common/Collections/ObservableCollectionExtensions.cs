using System.Collections.Generic;
using System.Linq;

namespace Philadelphia.Common {
    public static class ObservableCollectionExtensions {
        public static void Replace<T>(this IObservableCollection<T> self, IEnumerable<T> items) {
            self.Replace(items.ToArray());
        }

        public static void Append<T>(this IObservableCollection<T> self, T item) {
            self.InsertAt(self.Length, item);
        }

        public static void Delete<T>(this IObservableCollection<T> self, IEnumerable<T> items) {
            self.Delete(items.ToArray());
        }
    }
}
