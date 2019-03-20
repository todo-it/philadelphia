using System.Threading.Tasks;
using Philadelphia.Common;

namespace Philadelphia.Demo.SharedModel {
    [HttpService]
    public interface ITranslationsService {
        Task<TranslationItem[]> FetchTranslation(SupportedLang lng);
    }
}
