using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Demo.SharedModel;
using Philadelphia.Web;

namespace Philadelphia.Demo.Client {
    public class SseDemoFlow : IFlow<HTMLElement> {
        private readonly SseSenderForm _sender;
        private readonly SseListenerForm _listener;

        public SseDemoFlow(ISomeService someService) {
            _listener = new SseListenerForm(x => new ISomeService_ContinentalListener_SseSubscriber(x));
            _sender = new SseSenderForm(someService, () => _listener.SseSessionId);
        }

        public void Run(IFormRenderer<HTMLElement> renderer, Action atExit) {
            var panels = TwoPanelsWithResizerBuilder.BuildHorizontal(
                Hideability.None, false, renderer, Tuple.Create((int?)488, (int?)null));

            panels.First.ReplaceMaster(_sender);
            panels.FirstCanvas.LayoutMode = LayoutModeType.TitleExtra_Actions_Body;

            panels.Second.ReplaceMaster(_listener);
            panels.SecondCanvas.LayoutMode = LayoutModeType.TitleExtra_Actions_Body;

            renderer.ReplaceMasterWithAdapter(panels.Panel);

            _sender.Ended += (x, outcome) => {
                renderer.ClearMaster();
                atExit();
            };
        }
    }
}
