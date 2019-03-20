using System;

namespace Philadelphia.Common {
    public static class TypeExtensions {
        public static string FullNameWithoutGenerics(this Type self) {
            var inp = self.FullName;
            var i = inp.IndexOf('<');
            inp = i < 0 ? inp : inp.Substring(0, i);

            i = inp.IndexOf('`'); //new bridge.net
            inp = i < 0 ? inp : inp.Substring(0, i);

            i = inp.IndexOf('$'); //old bridge.net
            return i < 0 ? inp : inp.Substring(0, i);
        }
    }
}
