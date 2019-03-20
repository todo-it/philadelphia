using System;
using Bridge;
using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Web;

namespace SimpleValidation.Client {
    class SomeFormView : IFormView<HTMLElement> {
        public InputTypeButtonActionView Confirm = new InputTypeButtonActionView("OK").With(x => x.MarkAsFormsDefaultButton());
        public IView<HTMLElement>[] Actions => new []{Confirm};
        public InputView Inp = new InputView("Some entry");
        
        public RenderElem<HTMLElement>[] Render(HTMLElement parent) {
            //notice: you can mix text and controls safely thanks to  implicit conversion operators
            return new RenderElem<HTMLElement>[] {
                "<div style='font-size: 12px'>", 
                    Inp, 
                "</div>"
            };
        }
    }

    class SomeForm : IForm<HTMLElement,SomeForm,CompletedOrCanceled> {
        public string Title => "Example form";
        private readonly SomeFormView _view = new SomeFormView();
        public IFormView<HTMLElement> View => _view;
        public event Action<SomeForm, CompletedOrCanceled> Ended;
        public ExternalEventsHandlers ExternalEventsHandlers => 
            ExternalEventsHandlers.Create(() => Ended?.Invoke(this, CompletedOrCanceled.Canceled));
        //comment out above declaration and uncomment next line to make form noncloseable
//        public ExternalEventsHandlers ExternalEventsHandlers => ExternalEventsHandlers.Ignore;

        public SomeForm() {
            var inp = LocalValueFieldBuilder.Build(_view.Inp, 
                (v, errors) => errors.IfTrueAdd(string.IsNullOrWhiteSpace(v), 
                    "Must contain at least one non whitespace character"));
            
            var conf = LocalActionBuilder.Build(_view.Confirm, 
                () => Ended?.Invoke(this, CompletedOrCanceled.Completed));
            conf.BindEnableAndInitializeAsObserving(x => x.Observes(inp));
        }
    }

    public class Program {
        [Ready]
        public static void OnReady() {
            Toolkit.InitializeToolkit();
            var renderer = Toolkit.DefaultFormRenderer();
            
            var msg = new SomeForm();

            msg.Ended += (x, outcome) => renderer.Remove(x); 
            //outcome? for this form it's either Completed or Cancelled. For simplicity we don't do anything with it

            renderer.AddPopup(msg);
            //comment out former line and uncomment next line to achieve frameless
            //renderer.ReplaceMaster(msg); 
        }
    }
}
