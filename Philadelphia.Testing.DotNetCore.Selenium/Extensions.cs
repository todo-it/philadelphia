using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Philadelphia.Testing.DotNetCore.Selenium {
    public static class Extensions {
        public static IEnumerable<(int Index, T Value)> Indexed<T>(this IEnumerable<T> x) => x.Select((xx, i) => (i, xx));
    }
}
