using System;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class EnumChoiceForm<T> : IForm<HTMLElement,EnumChoiceForm<T>,CompletedOrCanceled> where T:struct {
        private readonly bool _isCancelable;
        private readonly EnumChoiceFormView<T> _view;
        private readonly LocalValue<T> _chosen;
        
        public event Action<EnumChoiceForm<T>,CompletedOrCanceled> Ended;
        public string Title {get; set;}
        public IFormView<HTMLElement> View => _view;
        public T ChosenValue => _chosen.Value;
        
        public EnumChoiceForm(
                string title, bool isCancelable, T defaultValue, Func<T,string> getLabel, 
                Action<EnumChoiceFormView<T>> postInitialization = null) {

            Title = title;
            _isCancelable = isCancelable;

            _view = new EnumChoiceFormView<T>(defaultValue, getLabel, postInitialization);
            _chosen = LocalValueFieldBuilder.BuildEnumBasedChoice(defaultValue, _view.Choice);
            
            LocalActionBuilder.Build(_view.Confirm, () => Ended?.Invoke(this, CompletedOrCanceled.Completed));
        }

        public ExternalEventsHandlers ExternalEventsHandlers =>
            _isCancelable
                ? ExternalEventsHandlers.Create(() => Ended?.Invoke(this, CompletedOrCanceled.Canceled))
                : ExternalEventsHandlers.Ignore;
    }
}
