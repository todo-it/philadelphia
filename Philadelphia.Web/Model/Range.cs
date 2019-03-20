namespace Philadelphia.Web {
    /// <summary>
    /// inclusive range
    /// </summary>
    public class Range {
        public int From { get; }
        public int To { get; }

        public bool IsNonEmpty => From >=0 && To >= From && To >= From;
        public int Length => 1+To-From; //inclusive range ex. from 5 to 5 there is 1 item

        private Range(int from, int to) {
            From = from;
            To = to;
        }

        public static Range CreateEmpty() {
            return new Range(-1, -2);
        }

        public static Range Create(int from, int to) {
            return new Range(from, to);
        }

        public override string ToString() {
            return string.Format("<Range empty?={0} from={1} to={2}>", !IsNonEmpty, From, To);
        }
    }
}
