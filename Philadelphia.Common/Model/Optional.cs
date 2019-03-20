using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphia.Common {
    /// <summary>
    /// F# option clone / something without value or having value / nullable for any type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Optional<T> {
        public T Value { get; }
        public bool HasValue { get; }

        private Optional(bool hasValue, T value) {
            HasValue = hasValue;
            Value = value;
        }

        public static Optional<T> CreateNone() {
            return new Optional<T>(false, default(T));
        }

        public static Optional<T> CreateSome(T value) {
            return new Optional<T>(true, value);
        }

        public override string ToString() {
            return $"<Optional hasValue={HasValue} value={Value}>";
        }
    }
}
