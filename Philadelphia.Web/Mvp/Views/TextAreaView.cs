using Bridge.Html5;

namespace Philadelphia.Web {
    public class TextAreaView : BaseInputView<string,HTMLTextAreaElement> {
        public string PlaceHolder {
            set => InputWidget.Placeholder = value;
        }

        public override bool Enabled { 
            protected get { return !InputWidget.ReadOnly; }
            set { InputWidget.ReadOnly = !value; } 
        }
        
        public TextAreaView(string label = "") : 
            base(
                new HTMLTextAreaElement(), 
                typeof(TextAreaView).FullName,
                x => x.Value,
                (x,v) => x.Value = v,
                label) {}
    }
}
