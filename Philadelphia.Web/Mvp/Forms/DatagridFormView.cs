using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class DatagridFormView : IFormView<HTMLElement> {
        public HtmlTableBasedTableView Items { get; } = new HtmlTableBasedTableView();
        public IView<HTMLElement>[] Actions => ActionsBuilder.For().AddFrom(Items);
        
        public RenderElem<HTMLElement>[] Render(HTMLElement _) => new RenderElem<HTMLElement>[] {Items};
    }
}
