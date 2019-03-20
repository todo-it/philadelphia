using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public interface ITabContent {
        void Add<T>(IReadOnlyValueView<HTMLElement, T> observed, HTMLElement view);
        void Add<T>(IReadOnlyValueView<HTMLElement,T> itm);
        void Add(HTMLElement itm);
    }
}
