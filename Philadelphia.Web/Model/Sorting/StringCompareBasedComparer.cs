using System.Collections.Generic;
using System.Globalization;
using Bridge;
using Philadelphia.Common;

namespace Philadelphia.Web {
    /// <summary>
    /// implementation note: apparently calling current implementation of 
    /// Bridge.System.String.Compare(string, string, bool, CultureInfo) is slow. for 5k items it takes 6sec. 
    /// Current implementation takes less than 1s
    /// </summary>
    public class StringCompareBasedComparer : IComparer<string> {
        [Template("{x}.localeCompare({y})")]
        public static extern int CompareImpl(string x, string y);
        
        public int Compare(string x, string y) {
            if (x != null) {
                return CompareImpl(x, y);
            }
            return y == null ? 0 : -1;
        }
    }
}
