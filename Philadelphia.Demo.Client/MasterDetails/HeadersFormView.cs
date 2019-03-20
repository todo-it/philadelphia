using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Web;

namespace Philadelphia.Demo.Client {
    public class HeadersFormView : IFormView<HTMLElement> {
        public HtmlTableBasedTableView Items {get; set; } = new HtmlTableBasedTableView();
        public IView<HTMLElement>[] Actions => ActionsBuilder.For().AddFrom(Items);
        public LabellessReadOnlyView Help = new LabellessReadOnlyView("div", TextType.TreatAsHtml)
            .WithCssClass("grayedOut")
            .WithCssClass("centered")
            .With(x => x.Value = @"Click on the row to choose master record and cause details population");

        public RenderElem<HTMLElement>[] Render(HTMLElement parentContainer) {
            //next two lines to eliminate space between Help and Items that confuses tbody height calculator
            parentContainer.Style.Display = Display.Flex;
            parentContainer.Style.FlexDirection = FlexDirection.Column; 

            return new RenderElem<HTMLElement>[] {Help,Items};
        }
    }
}
