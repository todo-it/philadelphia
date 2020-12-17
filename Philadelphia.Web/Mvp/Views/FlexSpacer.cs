using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class FlexSpacer : IView<HTMLElement> {
        private readonly HTMLElement _el = (new HTMLDivElement()).With(x => x.Style.Flex = "1");
        public HTMLElement Widget => _el;
    }
}
