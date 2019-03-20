using System;

namespace Philadelphia.Common {
    public class Base64Util {
        public static int GetEncodedLength(int rawLength) {
            var raw = rawLength/3.0m;
            return (int)Math.Ceiling(raw)*4;
        }
    }
}
