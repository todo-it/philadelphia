using System;
using System.Threading.Tasks;
using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Web;

namespace Philadelphia.Demo.Client {
   
    public class ValidationProgram : IFlow<HTMLElement> {
        public void Run(IFormRenderer<HTMLElement> renderer, Action atExit) {
            var input = new AllFieldsFilledDataEntryForm();
            var tryAgain = new ConfirmMessageForm(
                "Try again?", "Input done", TextType.TreatAsText, ConfirmLabels.YesNo);

            input.Ended.Add(x => {
                renderer.Remove(input);
                renderer.AddPopup(tryAgain);
            });

            tryAgain.Ended += (x, outcome) => {
                switch (outcome) {
                    case CompletedOrCanceled.Completed:
                        renderer.Remove(x);
                        renderer.AddPopup(input);
                        break;

                    case CompletedOrCanceled.Canceled:
                        renderer.Remove(x);
                        atExit();
                        break;

                    default: throw new Exception("unsupported outcome");
                }
            };

            renderer.AddPopup(input);
        }
    }
}
