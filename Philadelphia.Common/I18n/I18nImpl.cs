using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphia.Common {
    public interface I18nImpl {
        string Translate(string input);
        string TranslateForLang(string input, string lang);
    }
}
