using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphia.Common {
    public class NoTranslationI18n : I18nImpl {
        public string Translate(string input) {
            return input;
        }

        public string TranslateForLang(string input, string _) {
            return input;
        }
    }
}
