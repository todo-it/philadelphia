namespace Philadelphia.Common {
    /// <summary>
    /// F# Unit clone / something without no value or irrelevant value
    /// </summary>
    public class Unit {
		private Unit() {}
		public static readonly Unit Instance = new Unit();
	}
}
