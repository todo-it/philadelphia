using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphia.Common {
    public class TranslationWithFallbackI18n : I18nImpl {
        private readonly string _langName;
        private readonly IDictionary<string, string> _translation = new Dictionary<string, string>();

        public TranslationWithFallbackI18n(string langName, params TranslationItem[] translation) {
            _langName = langName;
            translation.ForEach(x => _translation.Add(x.M, x.T));
        }

        public string Translate(string input) {
            string value;

            if (_translation.TryGetValue(input, out value)) {
                return string.IsNullOrEmpty(value) ? input : value;
            }
            return input;
        }

        //NOTE: it is not really correct on client side
        public string TranslateForLang(string input, string lang) {
            return lang == _langName ? Translate(input) : input;
        }
    }
}
