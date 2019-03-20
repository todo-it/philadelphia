using Bridge.Html5;

namespace Philadelphia.Web {
    public class TabData {
        public HTMLElement TabLabel { get; set; }
        public HTMLElement TabHandle { get; set; }
        public HTMLElement TabContent { get; set; }

        public void SetLabel(string label) {
            TabLabel.TextContent = label;    
        }
    }
}
