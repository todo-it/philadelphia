using System;

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
    }
}
