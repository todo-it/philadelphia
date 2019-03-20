using System;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class ActionsBarMenuFormView : IMenuFormView {
        private readonly ActionButtonsMenuBarView _rawMenuBar;

        public IMenuBarView MenuBar => _rawMenuBar;
        public LabellessReadOnlyView BodyPanel = new LabellessReadOnlyView();
        public IView<HTMLElement>[] Actions => ActionsBuilder.For().AddFrom(_rawMenuBar);
        
        public ActionsBarMenuFormView(Func<MenuItemModel,InputTypeButtonActionView> customButtonBuilder = null) {
            _rawMenuBar = new ActionButtonsMenuBarView(customButtonBuilder).With(x => x.DecorateAsFormView());
        }

        public RenderElem<HTMLElement>[] Render(HTMLElement _) {
            return new RenderElem<HTMLElement>[] {BodyPanel};
        }
    }
}
