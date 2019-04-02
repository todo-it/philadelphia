using System.Linq;

namespace ControlledByTests.Api {
    public static class ArrayExtensions {
        //TODO: once bridge.net supports Linq's Zip, move it into Philadelphia.Common
        public static bool IsTheSameAs(this object[] first, object[] second) {
            return
                first.Length == second.Length &&
                first
                    .Zip(second, (f,s) => (f,s))
                    .All(x => x.Item1 != null && x.Item1.Equals(x.Item2) || 
                              x.Item1 == null && x.Item2 == null);
        }
    }
}
