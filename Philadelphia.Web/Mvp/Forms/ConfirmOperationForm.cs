using System;
using System.Threading.Tasks;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class ConfirmOperationForm<T> :
            IOnShownNeedingForm<HTMLElement>, 
            IForm<HTMLElement, ConfirmOperationForm<T>, ConfirmOperationFormOutcome> {

        public event Action<ConfirmOperationForm<T>,ConfirmOperationFormOutcome> Ended;
        
        private readonly IReadWriteValue<string> _message;
        private readonly ConfirmOperationFormView _view;
        public string Title { get; private set; }
        public IFormView<HTMLElement> View => _view;
        public T SuccessResult {get; private set; }

        public ConfirmOperationForm(Func<Task<T>> operation, TextType textType, string message = null, string title = null) :
            this(operation, new ConfirmOperationFormView(textType), message, title) {}

        public ConfirmOperationForm(Func<Task<T>> operation, string message = null, string title = null) :
            this(operation, new ConfirmOperationFormView(), message, title) {}

        public ConfirmOperationForm(Func<Task<T>> operation, ConfirmOperationFormView view, string message = null, string title = null) {
            Title = title ?? I18n.Translate("Confirmation");
            _view = view;
            
            _message = new LocalValue<string>(message ?? I18n.Translate("Without message"));
            view.Message.BindReadOnlyAndInitialize(_message);
            
            RemoteActionBuilder.Build(view.Confirm, operation, x => {
                SuccessResult = x;
                Ended?.Invoke(this, ConfirmOperationFormOutcome.Success);
            });
        }
        
        public async Task Init(string message, string title = null) {
            
            Title = title ?? I18n.Translate("Confirmation");
            await _message.DoChange(message, false, this, false);
        }

        public ExternalEventsHandlers ExternalEventsHandlers => 
            ExternalEventsHandlers.Create(() => Ended?.Invoke(this, ConfirmOperationFormOutcome.FailureOrCanceled));

        public void OnShown() {
            _view.Confirm.State = ActionViewState.CreateIdleOrSuccess(); //don't show former error
        }
    }
}
