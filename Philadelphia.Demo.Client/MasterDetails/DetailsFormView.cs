using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Web;

namespace Philadelphia.Demo.Client {
    public class DetailsFormView :IFormView<HTMLElement> {
        public HtmlTableBasedTableView Items {get; set; } = new HtmlTableBasedTableView();
        public IView<HTMLElement>[] Actions => ActionsBuilder.For().AddFrom(Items);

        public RenderElem<HTMLElement>[] Render(HTMLElement parentContainer) {
            //next two lines to eliminate space between Help and Items that confuses tbody height calculator
            parentContainer.Style.Display = Display.Flex;
            parentContainer.Style.FlexDirection = FlexDirection.Column; 

            return new RenderElem<HTMLElement>[] {Items}; 
        }
    }
}
