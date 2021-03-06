using System;
using System.Threading.Tasks;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class InformationalMessageForm : IForm<HTMLElement,InformationalMessageForm,Unit> {
        private readonly bool _cancellable;
        public event Action<InformationalMessageForm,Unit> Ended;
        
        private readonly LocalValue<string> _message;
        public string Title { get; private set; }
        public IFormView<HTMLElement> View { get; }

        public ExternalEventsHandlers ExternalEventsHandlers => 
            _cancellable 
            ? ExternalEventsHandlers.Create(() => Ended?.Invoke(this, Unit.Instance))
            : ExternalEventsHandlers.Ignore;
        
        public InformationalMessageForm(string messageOrNull = null, string titleOrNull = null, TextType textType = TextType.TreatAsText) 
            : this(new InformationalMessageFormView(textType), messageOrNull, titleOrNull) {
        }

        public InformationalMessageForm(InformationalMessageFormView view, string messageOrNull = null, string titleOrNull = null, bool cancellable = true) {
            _cancellable = cancellable;
            Title = titleOrNull ?? I18n.Translate("Confirmation");
            View = view;

            _message = new LocalValue<string>(messageOrNull ?? I18n.Translate("Without message"));
            view.Message.BindReadOnlyAndInitialize(_message);

            LocalActionBuilder.Build(view.Confirm, () => Ended?.Invoke(this, Unit.Instance));
        }
        
        public async Task Init(string message, string title = null) {
            Title = title ?? Title ?? I18n.Translate("Untitled");
            await _message.DoChange(message, false, this, false);
        }
    }
}
