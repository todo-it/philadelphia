using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Philadelphia.Common;
using Philadelphia.Demo.SharedModel;

namespace Philadelphia.Demo.ServicesImpl {
    public class TranslationService : ITranslationsService {
        public Task<TranslationItem[]> FetchTranslation(SupportedLang lang) {
            if (lang == SupportedLang.EN) {
                throw new Exception("no need to download english translations");
            }

            return Task.FromResult(
                JsonConvert.DeserializeObject<TranslationRoot>(
                    File.ReadAllText(
                        $"Translations/translation_{lang.ToString()}.json")
                ).Items);
        }
    }
}
