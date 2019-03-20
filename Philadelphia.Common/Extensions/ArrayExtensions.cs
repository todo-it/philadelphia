using System;

namespace Philadelphia.Common {
    public static class ArrayExtensions {
        /// <summary>foreach that returns self so that it can be used as expression (iso of statement)</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="operation"></param>
        /// <returns></returns>
        public static T[] ForEachFluent<T>(this T[] self, Action<T> operation) {
            foreach (var t in self) {
                operation(t);
            }
            return self;
        }
    }
}
