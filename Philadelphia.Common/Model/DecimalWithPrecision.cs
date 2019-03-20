
using System;
using System.Collections.Generic;

namespace Philadelphia.Common {
    public class DecimalWithPrecision {
        public int Precision {get; }
        public decimal Value {get; }
        public decimal RoundedValue => Math.Round(Value, Precision);

        public DecimalWithPrecision(decimal value, int precision) {
            Value = value;
            Precision = precision;
        }

        public override bool Equals(object o) {
            if (!(o is DecimalWithPrecision)) {
                return false;
            }

            var other = (DecimalWithPrecision)o;
            return RoundedValue.Equals(other.RoundedValue);
        }

        public override int GetHashCode() {
            return Value.GetHashCode();
        }

        public override string ToString() {
            return $"<DecimalWithPrecision Value={Value} Precision={Precision}>";
        }

        public static int ComparatorImpl(DecimalWithPrecision x, DecimalWithPrecision y) {
            if (x == null) {
                return y == null ? 0 : -1;
            }

            return y == null ? 1 : x.RoundedValue.CompareTo(y.RoundedValue);
        }
    }

    public class DecimalWithPrecisionDefaultComparer : IComparer<DecimalWithPrecision> {
        public int Compare(DecimalWithPrecision x, DecimalWithPrecision y) {
            return DecimalWithPrecision.ComparatorImpl(x, y);
        }
    }
}
