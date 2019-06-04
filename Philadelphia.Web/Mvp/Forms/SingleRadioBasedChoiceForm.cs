using System;
using System.Collections.Generic;
using Bridge.Html5;
using Philadelphia.Common;
using System.Linq;

namespace Philadelphia.Web {
    public class SingleRadioBasedChoiceForm<T> : IForm<HTMLElement,SingleRadioBasedChoiceForm<T>,CompletedOrCanceled> {
        private readonly bool _isCancelable;
        private readonly Func<T, string> _getLabel;
        private readonly Func<T, int> _itemToInt;
        private readonly SingleRadioBasedChoiceFormView<T> _view;
        private readonly LocalValue<T> _chosen;
        
        public event Action<SingleRadioBasedChoiceForm<T>,CompletedOrCanceled> Ended;
        public string Title {get; set;}
        public IFormView<HTMLElement> View => _view;
        public T ChosenValue => _chosen.Value;
        
        public SingleRadioBasedChoiceForm(
                string title, bool isCancelable, T defaultValue, 
                Func<T,string> getLabel, Func<int,T> intToItem, Func<T,int> itemToInt,
                Action<SingleRadioBasedChoiceFormView<T>> postInitialization = null) {

            Title = title;
            _isCancelable = isCancelable;
            _getLabel = getLabel;
            _itemToInt = itemToInt;

            _view = new SingleRadioBasedChoiceFormView<T>(
                defaultValue, getLabel, intToItem, itemToInt, postInitialization);
            _chosen = LocalValueFieldBuilder.BuildGeneralChoice(
                defaultValue, intToItem, itemToInt, _view.Choice);
            
            LocalActionBuilder.Build(_view.Confirm, () => Ended?.Invoke(this, CompletedOrCanceled.Completed));
        }
        
        public void PermittedValuesInit(IEnumerable<T> values) {
            _view.Choice.PermittedValues = values.Select(x => Tuple.Create(_itemToInt(x)+"", _getLabel(x)));
        }

        public ExternalEventsHandlers ExternalEventsHandlers =>
            _isCancelable
                ? ExternalEventsHandlers.Create(() => Ended?.Invoke(this, CompletedOrCanceled.Canceled))
                : ExternalEventsHandlers.Ignore;
    }
}
