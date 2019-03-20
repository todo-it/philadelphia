using System;
using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Web;

namespace Philadelphia.Web {
    public class EnumChoiceForm<T> : IForm<HTMLElement,EnumChoiceForm<T>,EnumChoiceFormOutcome> where T:struct {
        private readonly bool _isCancelable;
        private readonly EnumChoiceFormView<T> _view;
        private readonly LocalValue<T> _choosen;

        public event Action<EnumChoiceForm<T>,EnumChoiceFormOutcome> Ended;
        public string Title {get; set;}
        public IFormView<HTMLElement> View => _view;
        public T ChoosenValue => _choosen.Value;
        
        public EnumChoiceForm(
                string title, bool isCancelable, T defaultValue, 
                Func<T,string> getLabel, Func<int,T> intToEnum, 
                Action<EnumChoiceFormView<T>> postInitialization = null) {

            Title = title;
            _isCancelable = isCancelable;

            _view = new EnumChoiceFormView<T>(defaultValue, getLabel, intToEnum, postInitialization);
            _choosen = LocalValueFieldBuilder.BuildEnumBasedChoice(defaultValue, _view.Choice);
            
            LocalActionBuilder.Build(_view.Confirm, () => Ended?.Invoke(this, EnumChoiceFormOutcome.Choosen));
        }

        public ExternalEventsHandlers ExternalEventsHandlers =>
            _isCancelable
                ? ExternalEventsHandlers.Create(() => Ended?.Invoke(this, EnumChoiceFormOutcome.Canceled))
                : ExternalEventsHandlers.Ignore;
    }
}
