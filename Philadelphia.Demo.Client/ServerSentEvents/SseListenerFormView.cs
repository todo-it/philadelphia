using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Web;

namespace Philadelphia.Demo.Client {
    public class SseListenerFormView : IFormView<HTMLElement> {
        public IView<HTMLElement>[] Actions => 
            ActionsBuilder.For(SubscribeEurope, SubscribeAfrica, SubscribeNorthAmerica, 
                SubscribeAntarctica, Unsubscribe);
        public LabeledReadOnlyView HistoryLog {get; }
            = new LabeledReadOnlyView("History (newest first):").With(x => {
                x.ContentElement.Style.Border = "0.5px solid gray";
                x.ContentElement.Style.MinWidth = "200px";
                x.ContentElement.Style.Height = "calc(100vh - 230px)";
                x.ContentElement.Style.Overflow = Overflow.Scroll; 
                x.ContentElement.Style.Display = Display.Flex;
                x.ContentElement.Style.FlexDirection = FlexDirection.Column; });
        public InputTypeButtonActionView Unsubscribe {get; }
            = new InputTypeButtonActionView("Stop subscription");
        public InputTypeButtonActionView SubscribeEurope {get; }
            = new InputTypeButtonActionView("Europe");
        public InputTypeButtonActionView SubscribeAfrica {get; }
            = new InputTypeButtonActionView("Africa");
        public InputTypeButtonActionView SubscribeNorthAmerica {get; }
            = new InputTypeButtonActionView("North America");
        public InputTypeButtonActionView SubscribeAntarctica {get; }
            = new InputTypeButtonActionView("Antarctica");

        public RenderElem<HTMLElement>[] Render(HTMLElement parentContainer) {
            return new RenderElem<HTMLElement>[] {
                "<div class='grayedOut' style='min-height: 70px'>Subscribe to notifications about countries on specific continents. On another computer or in another browser window send some notifications and they will appear here. If connection is interrupted(f.e. network is down) it is automatically resumed. <br>Antarctica will not work by design to show that subscriber rejection scenario is handled.</div><br><br>", 
                HistoryLog};
        }
    }
}
