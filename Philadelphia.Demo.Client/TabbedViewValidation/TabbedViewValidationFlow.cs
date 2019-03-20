using System;
using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Web;

namespace Philadelphia.Demo.Client {
    public class TabbedViewValidationFlow : IFlow<HTMLElement> {
        private readonly InformationalMessageForm _msg;
        private readonly IntroduceItemForm _input;

        public TabbedViewValidationFlow() {
            _input = new IntroduceItemForm();

            var confirmView = new InformationalMessageFormView();
            confirmView.Message.Widget.AddClasses(Magics.CssClassPreserveNewlines);
            _msg = new InformationalMessageForm(confirmView);
        }

        public void Run(IFormRenderer<HTMLElement> renderer, Action atExit) {
            renderer.AddPopup(_input);
            _input.Ended += async (x,outcome) => {
                renderer.Remove(x);

                switch(outcome) {
                    case IntroduceItemForm.Outcome.Saved:
                        await _msg.Init($@"
SomeNumber: {x.CreatedItem.SomeNumber}
SomeText: {x.CreatedItem.SomeText}
SomeBool: {x.CreatedItem.SomeBool}
SomeTrait: enum={x.CreatedItem.SomeTrait.ToString()} int={x.CreatedItem.SomeTrait}", "This would be saved..." );
                        renderer.AddPopup(_msg);
                        break;

                    case IntroduceItemForm.Outcome.Canceled: break;
                    default: throw new Exception("unsupported outcome");
                }
            };
            
            _msg.Ended += (x, _) => {
                renderer.Remove(x);
                atExit();
            };
        }
    }
}
