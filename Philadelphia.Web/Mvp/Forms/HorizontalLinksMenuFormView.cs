using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class HorizontalLinksMenuFormView : IMenuFormView {
        private readonly HorizontalMenuBarView _rawMenuBar = new HorizontalMenuBarView().With(x => x.DecorateAsFormView());
        public IMenuBarView MenuBar => _rawMenuBar;
        public LabellessReadOnlyView BodyPanel = new LabellessReadOnlyView();
        public IView<HTMLElement>[] Actions => new IView<HTMLElement>[] {};
        
        public RenderElem<HTMLElement>[] Render(HTMLElement _) {
            return new RenderElem<HTMLElement>[] {
                "<div style='display: flex; flex-direction: column'>", _rawMenuBar, BodyPanel, "</div>"
            };
        }
    }
}
