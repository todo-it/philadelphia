using System;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Demo.Client {
    public class SomeChoicesForm : IForm<HTMLElement,SomeChoicesForm,SomeChoicesForm.Outcome> {
        public enum Outcome {
            FirstChoice,
            SecondChoice,
            Canceled
        }
        public event Action<SomeChoicesForm,Outcome> Ended;
        public string Title => "Multiple outcomes";
        private readonly SomeChoicesFormView _view;
        public IFormView<HTMLElement> View => _view;
        
        public SomeChoicesForm() {
            _view = new SomeChoicesFormView();

            LocalActionBuilder.Build(_view.First, () => Ended?.Invoke(this, Outcome.FirstChoice));
            LocalActionBuilder.Build(_view.Second, () => Ended?.Invoke(this, Outcome.SecondChoice));
        }

        //public ExternalEventsHandlers ExternalEventsHandlers => ExternalEventsHandlers.Ignore;
        public ExternalEventsHandlers ExternalEventsHandlers => ExternalEventsHandlers.Create(() => Ended?.Invoke(this, Outcome.Canceled));
    }
}
