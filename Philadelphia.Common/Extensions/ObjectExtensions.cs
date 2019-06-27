using System;
using System.Linq;

namespace Philadelphia.Common {
    public static class ObjectExtensions {
        public static T With<T>(this T self, Action<T> oper) {
            oper(self);
            return self;
        }

        public static ResultT Then<T, ResultT>(this T item, Func<T, ResultT> mapper) => mapper(item);

        public static bool IsSameAs(object first, object second) {
            return 
                first == null && second == null || 
                first != null && first.Equals(second);
        }

		/// <summary>provides concise check if value Equals anything in the list</summary>
        public static bool EqualsToAny<T>(this T self, params T[] others) {
            return others.Any(x => x.Equals(self));
        }
    }
}
