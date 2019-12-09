namespace Philadelphia.Common {
    /// <summary>
    /// F# Unit clone / something without no value or irrelevant value
    /// </summary>
    public class Unit {
        /// <summary>
        /// don't use directly. It is public to make JSON deserialization working
        /// </summary>
        public Unit() {}
		public static readonly Unit Instance = new Unit();

        public override bool Equals(object obj) {
            return obj is Unit;
        }

        public override int GetHashCode() {
            return 0;
        }
    }
}
