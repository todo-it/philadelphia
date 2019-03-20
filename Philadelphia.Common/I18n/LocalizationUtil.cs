using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphia.Common {
    public class LocalizationUtil {
        public static string BoolToUnicodeCheckbox(bool inp) {
            return inp ? "☑" : "☐";
        }
    }
}
