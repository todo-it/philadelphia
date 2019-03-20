using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Web;

namespace Philadelphia.Demo.Client {
    public class SseSenderFormView : IFormView<HTMLElement> {
        public IView<HTMLElement>[] Actions => ActionsBuilder.For(
            PublishGermany, PublishFrance, PublishCanada, PublishUSA, PublishSouthAfrica, PublishTunisia);
        public LabeledReadOnlyView HistoryLog {get; }
            = new LabeledReadOnlyView("History (newest first):").With(x => {
                x.ContentElement.Style.Border = "0.5px solid gray";
                x.ContentElement.Style.MinWidth = "200px";
                x.ContentElement.Style.Height = "calc(100vh - 230px)";
                x.ContentElement.Style.Overflow = Overflow.Scroll;
                x.ContentElement.Style.Display = Display.Flex;
                x.ContentElement.Style.FlexDirection = FlexDirection.Column; });
        public InputTypeButtonActionView PublishGermany {get; }
            = new InputTypeButtonActionView("Germany");
        public InputTypeButtonActionView PublishUSA {get; }
            = new InputTypeButtonActionView("USA");
        public InputTypeButtonActionView PublishCanada {get; }
            = new InputTypeButtonActionView("Canada");
        public InputTypeButtonActionView PublishFrance {get; }
            = new InputTypeButtonActionView("France");
        public InputTypeButtonActionView PublishSouthAfrica {get; }
            = new InputTypeButtonActionView("South Africa");
        public InputTypeButtonActionView PublishTunisia {get; }
            = new InputTypeButtonActionView("Tunisia");

        public RenderElem<HTMLElement>[] Render(HTMLElement parentContainer) {
            return new RenderElem<HTMLElement>[] {
                "<div class='grayedOut' style='min-height: 70px'>Publish some notifications and they will appear in the right pane. This will work on other computers as well.</div><br><br>",HistoryLog};
        }
    }
}
