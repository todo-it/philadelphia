using System;
using System.Collections.Generic;
using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Web;

namespace Philadelphia.Tests.Client.UI {
    public class TestRunnerView : IFormView<HTMLElement> {
        public IView<HTMLElement>[] Actions => new IView<HTMLElement>[]{ };
        public HtmlTableBasedTableView Grid { get; } = 
            new HtmlTableBasedTableView()
                .WithStyle(new Dictionary<string, string>{["overflow-x"] = "scroll"});
        private readonly LabellessReadOnlyView _summaryView = new LabellessReadOnlyView(inputType: TextType.TreatAsPreformatted);
        private readonly IView<HTMLElement> _splitter;
        public TestRunnerView() {
            var x = new TwoHorizontalPanelsWithResizer(Hideability.None, new Tuple<int?, int?>(1100, null));
            x.FirstPanel.AppendChild(Grid.Widget);
            x.SecondPanel.AppendChild(_summaryView.Widget);
            
            _splitter = x;
        }

        public string SummaryText {
            get => _summaryView.Value;
            set => _summaryView.Value = value;
        }

        public RenderElem<HTMLElement>[] Render(HTMLElement parentContainer) {
            return new[] {
                "<div>Please refresh page to re-run tests</div>",
                RenderElem<HTMLElement>.Create(_splitter),
            };
        }
    }
}
