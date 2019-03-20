using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    class AdaptAsIview : IView<HTMLElement> {
        public HTMLElement Widget { get; }

        public AdaptAsIview(HTMLElement elem) => Widget = elem;
    }
}
