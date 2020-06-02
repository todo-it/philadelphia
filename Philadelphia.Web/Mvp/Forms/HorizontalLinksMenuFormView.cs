using System;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class HorizontalLinksMenuFormView : IMenuFormView {
        private readonly HorizontalMenuBarView _rawMenuBar;
        public IMenuBarView MenuBar => _rawMenuBar;
        public LabellessReadOnlyView BodyPanel = new LabellessReadOnlyView();
        public IView<HTMLElement>[] Actions => new IView<HTMLElement>[] {};
        
        public HorizontalLinksMenuFormView(
                Func<MenuItemModel,Tuple<HTMLElement,Action<string>>> customItemBuilder = null) {

            _rawMenuBar = new HorizontalMenuBarView(customItemBuilder);
        }

        public RenderElem<HTMLElement>[] Render(HTMLElement _) {
            return new RenderElem<HTMLElement>[] {
                "<div style='display: flex; flex-direction: column'>", _rawMenuBar, BodyPanel, "</div>"
            };
        }
    }
}
