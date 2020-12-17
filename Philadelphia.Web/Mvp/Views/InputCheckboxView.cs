using Bridge.Html5;

namespace Philadelphia.Web {
    public class InputCheckboxView : BaseInputView<bool,HTMLInputElement> {
        public override bool Enabled { 
            protected get { return !InputWidget.Disabled; }
            set { InputWidget.Disabled = !value; } 
        }
        
        public InputCheckboxView(string label) : 
            base(
                new HTMLInputElement().WithAttribute("type", "checkbox"), 
                typeof(InputView).FullName,
                x => x.Checked,
                (x,v) => x.Checked = v,
                label) {}
        
        public void SetAsInputThenLabel() {
            Widget.Style.Display = Display.Flex;
            Widget.Style.AlignItems = "center";
            Widget.Children[0].Style.Order = "2";
        }
    }
}
