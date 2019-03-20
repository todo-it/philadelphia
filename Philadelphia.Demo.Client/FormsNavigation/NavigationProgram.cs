using System;
using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Web;

namespace Philadelphia.Demo.Client {
    class NavigationProgram : IFlow<HTMLElement> {
        private readonly SomeChoicesForm _dataEntry;
        private readonly InformationalMessageForm _msg;

        public NavigationProgram() {
            _dataEntry = new SomeChoicesForm();
            _msg = new InformationalMessageForm("", "Outcome display");
        }

        public void Run(IFormRenderer<HTMLElement> renderer, Action atExit) {
            renderer.AddPopup(_dataEntry);

            _dataEntry.Ended += async (x, outcome) => {
                Logger.Debug(GetType(), "_dataEntry ended with outcome {0}", outcome);
                renderer.Remove(x);

                switch (outcome) {
                    case SomeChoicesForm.Outcome.Canceled:
                        await _msg.Init("You've canceled form");
                        break;

                    case SomeChoicesForm.Outcome.FirstChoice:
                        await _msg.Init("You've picked first choice");
                        break;

                    case SomeChoicesForm.Outcome.SecondChoice:
                        await _msg.Init("You've picked second choice");
                        break;

                    default: throw new Exception("unsupported outcome");
                }
                renderer.AddPopup(_msg);
            };

            _msg.Ended += (x, unit) => {
                Logger.Debug(GetType(), "_msg ended with outcome {0}", unit);

                renderer.Remove(x);
                atExit();
            };
        }
    }
}
