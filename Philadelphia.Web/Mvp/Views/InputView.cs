using Bridge.Html5;

namespace Philadelphia.Web {
    public class InputView : BaseInputView<string,HTMLInputElement> {
        public const string TypeText = "text";
        public const string TypePassword = "password";
        public const string TypeNumber = "number";
        
        public string PlaceHolder {
            set => InputWidget.Placeholder = value;
        }

        public override bool Enabled { 
            protected get { return !InputWidget.ReadOnly; }
            set { InputWidget.ReadOnly = !value; } 
        }

        public InputView(string label = "", string inputType = TypeText) : 
                base(
                    new HTMLInputElement().WithAttribute("type", inputType), 
                    typeof(InputView).FullName,
                    x => x.Value,
                    (x,v) => x.Value = v,
                    label) {

            InputWidget.SetBoolAttribute(Magics.AttrDataHandlesEnter, true);
        }
    }
}
