using System;
using System.Linq;
using System.Threading.Tasks;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class TextInputForm : IForm<HTMLElement,TextInputForm,CompletedOrCanceled> {
        public event Action<TextInputForm,CompletedOrCanceled> Ended;
        public string Title { get; private set; }
        public IFormView<HTMLElement> View => _view;
        public string Introduced => _input.Value;

        private readonly IReadWriteValue<string> _msg;
        private readonly LocalValue<string> _input;
        private readonly TextInputFormView _view;

        public TextInputForm(
            string labelOrNull = null, string titleOrNull = null, TextType textType = TextType.TreatAsText,string defaultValue = null,
            params Validate<string>[] validators) 
            : this(new TextInputFormView(textType), labelOrNull, titleOrNull, defaultValue, validators) {
        }

        public TextInputForm(
            TextInputFormView view, string label, string titleOrNull = null, string defaultValue = null,
            params Validate<string>[] validators) {

            Title = titleOrNull ?? I18n.Translate("Input");
            _view = view;

            _msg = new LocalValue<string>(label ?? "");
            view.Label.BindReadOnlyAndInitialize(_msg);

            _input = LocalValueFieldBuilder.Build(defaultValue, view.Input, validators);

            var isFormValid = new AggregatedErrorsValue<bool>(false, x => !x.Errors.Any(), x => x.Observes(_input));
            var confirmInput = LocalActionBuilder.Build(view.Confirm, () => Ended?.Invoke(this, CompletedOrCanceled.Completed));
            confirmInput.BindEnableAndInitialize(isFormValid);
        }
        
        public async Task Init(string label, string message = null, string title = null) {
            if (title != null) {
                Title = title;
            }
            
            _view.Input.Label = label;
            await _msg.DoChange(message, false, this, false);
            _input.Reset(false, this);
        }

        public void Reset() {
            _input.Reset(false, this);
        }

        public ExternalEventsHandlers ExternalEventsHandlers => 
            ExternalEventsHandlers.Create(() => Ended?.Invoke(this, CompletedOrCanceled.Canceled));
    }
}
