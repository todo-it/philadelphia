using System;
using System.Threading.Tasks;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class ConfirmMessageForm : IForm<HTMLElement,ConfirmMessageForm,CompletedOrCanceled> {
        public event Action<ConfirmMessageForm,CompletedOrCanceled> Ended;

        private readonly IReadWriteValue<string> _message;
        public string Title { get; private set; }
        public IFormView<HTMLElement> View { get; }

        public ConfirmMessageForm(
                string messageOrNull = null, string titleOrNull = null, TextType textType = TextType.TreatAsText,
                ConfirmLabels labels = ConfirmLabels.ConfirmCancel)

            : this(new ConfirmMessageFormView(textType, labels), messageOrNull, titleOrNull) {
        }

        public ConfirmMessageForm(ConfirmMessageFormView view, string messageOrNull = null, string titleOrNull = null) {			
            Title = titleOrNull ?? I18n.Translate("Confirmation");
            View = view;

            _message = new LocalValue<string>(messageOrNull ?? I18n.Translate("Without message"));
            view.Message.BindReadOnlyAndInitialize(_message);

            LocalActionBuilder.Build(view.Confirm, () => Ended?.Invoke(this, CompletedOrCanceled.Completed));
            LocalActionBuilder.Build(view.Cancel, () => Ended?.Invoke(this, CompletedOrCanceled.Canceled));
        }
        
        public async Task Init(string message, string title = null) {
            Title = title ?? I18n.Translate("Untitled");
            await _message.DoChange(message, false, this, false);
        }

        public ExternalEventsHandlers ExternalEventsHandlers => ExternalEventsHandlers.Create(() => Ended?.Invoke(this, CompletedOrCanceled.Canceled));
    }
}
