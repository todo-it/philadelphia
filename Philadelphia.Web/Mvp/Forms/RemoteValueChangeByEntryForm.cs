using System;
using System.Linq;
using System.Threading.Tasks;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class RemoteValueChangeByEntryForm<DomainT,ViewT,RemoteT> : IForm<HTMLElement,RemoteValueChangeByEntryForm<DomainT,ViewT,RemoteT>,CompletedOrCanceled> {
        private readonly Action<string> _inputLabelChanger;
        public event Action<RemoteValueChangeByEntryForm<DomainT,ViewT,RemoteT>,CompletedOrCanceled> Ended;
        public string Title { get; private set; }
        public IFormView<HTMLElement> View => _view;
        public DomainT Introduced => _input.Value;
        public RemoteT SavedValue {get; private set;}

        private readonly IReadWriteValue<string> _msg;
        private readonly LocalValue<DomainT> _input;
        private readonly RemoteValueChangeByEntryFormView<ViewT> _view;

        public RemoteValueChangeByEntryForm(
                Func<DomainT,Task<RemoteT>> remoteChanger,
                IReadWriteValueView<HTMLElement,ViewT> input,
                Action<string> inputLabelChanger,
                Func<DomainT,ViewT> domainToViewConverter,
                Func<ViewT,DomainT> viewToDomainConverter,
                string label, string titleOrNull = null, 
                DomainT defaultValue = default(DomainT),
                TextType labelTextType = TextType.TreatAsText,
                params Validate<DomainT>[] validators) {

            _inputLabelChanger = inputLabelChanger;

            Title = titleOrNull ?? I18n.Translate("Input");
            _view = new RemoteValueChangeByEntryFormView<ViewT>(input, labelTextType);

            _inputLabelChanger(label);
            _msg = new LocalValue<string>("");
            _view.Message.BindReadOnlyAndInitialize(_msg);

            _input = LocalValueFieldBuilder.Build(
                defaultValue, _view.Input, domainToViewConverter, viewToDomainConverter, validators);

            var isFormValid = new AggregatedErrorsValue<bool>(
                false, x => !x.Errors.Any(), x => x.Observes(_input));

            var confirmInput = RemoteActionBuilder.Build(
                _view.Confirm, 
                () => remoteChanger(_input.Value),
                x => {
                    SavedValue = x;
                    Ended?.Invoke(this, CompletedOrCanceled.Completed); });

            confirmInput.BindEnableAndInitialize(isFormValid);
        }
        
        public async Task Init(string message = null, string label = null, string title = null) {
            if (message != null) {
                await _msg.DoChange(message, false, this, false);
            }
            
            if (label != null) {
                _inputLabelChanger(label);
            }

            if (title != null) {
                Title = title;
            }
            
            _input.Reset(false, this);
        }

        public ExternalEventsHandlers ExternalEventsHandlers => 
            ExternalEventsHandlers.Create(() => Ended?.Invoke(this, CompletedOrCanceled.Canceled));

        public static RemoteValueChangeByEntryForm<string,string,RemT> CreateTextEntry<RemT>(
            Func<string,Task<RemT>> remoteChanger, string label, string titleOrNull = null, 
            string defaultValue = "", TextType labelTextType = TextType.TreatAsText,
            params Validate<string>[] validators) {

            var input = new InputView();

            return new RemoteValueChangeByEntryForm<string,string,RemT>(
                remoteChanger,
                input, 
                x => input.Label = x,
                x => x,
                x => x,
                label, 
                titleOrNull,
                defaultValue,
                labelTextType,
                validators);
        }
        
        public static RemoteValueChangeByEntryForm<DomT,string,RemT> CreateTextBasedEntry<DomT,RemT>(
                Func<DomT,Task<RemT>> remoteChanger, 
                Func<DomT,string> domainToView,
                Func<string,DomT> viewToDomain,
                string label, string titleOrNull = null, 
                DomT defaultValue = default(DomT), TextType labelTextType = TextType.TreatAsText,
                params Validate<DomT>[] validators) {

            var input = new InputView(label);

            return new RemoteValueChangeByEntryForm<DomT,string,RemT>(
                remoteChanger,
                input, 
                x => input.Label = x,
                domainToView,
                viewToDomain,
                label, 
                titleOrNull,
                defaultValue,
                labelTextType,
                validators);
        }
    }
}
