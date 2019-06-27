using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class RemoteValueChangeByChoiceForm<T> : IForm<HTMLElement,RemoteValueChangeByChoiceForm<T>,CompletedOrCanceled> where T:new() {
        private readonly RemoteValueChangeByChoiceFormView<T> _view;
        private readonly LocalValue<T> _localValue;
        private readonly Func<string> _titleProv;

        public event Action<RemoteValueChangeByChoiceForm<T>,CompletedOrCanceled> Ended;
        public string Title => _titleProv();
        public IFormView<HTMLElement> View => _view;
        public T SavedValue { get; private set; }
        
        public RemoteValueChangeByChoiceForm(
                string title, Func<T,Task<T>> saveChange, 
                RemoteValueChangeByChoiceFormView<T> view,
                params Validate<T>[] validators)
                    : this(() => title, saveChange, view, validators) {}

        public RemoteValueChangeByChoiceForm(
                Func<string> titleProv, Func<T,Task<T>> saveChange, 
                RemoteValueChangeByChoiceFormView<T> view,
                params Validate<T>[] validators) {

            _titleProv = titleProv;
            _view = view;
            
            _localValue = LocalValueFieldBuilder.Build(default(T), view.Choosen, validators);
            
            var remoteActionModel = RemoteActionBuilder.Build(view.Confirm,
                () => saveChange(_localValue.Value),
                x => {
                    SavedValue = x;
                    Ended?.Invoke(this, CompletedOrCanceled.Completed); });

            var isFormValid = new AggregatedErrorsValue<bool>(false, self => !self.Errors.Any(), x => {
                x.Observes(_localValue); });

            remoteActionModel.BindEnableAndInitialize(isFormValid);
        }
        
        public void Init(IEnumerable<T> permittedValues) {
            _view.Confirm.State = ActionViewState.CreateIdleOrSuccess();
            _localValue.Reset(false, this);
            _view.Choosen.PermittedValues = permittedValues;
        }
        
        public async Task Init(IEnumerable<T> permittedValues, T currentValue) {
            _view.Confirm.State = ActionViewState.CreateIdleOrSuccess();
            await _localValue.DoChange(currentValue, false, this, false);
            _view.Choosen.PermittedValues = permittedValues;
        }

        public ExternalEventsHandlers ExternalEventsHandlers => 
            ExternalEventsHandlers.Create(() => Ended?.Invoke(this, CompletedOrCanceled.Canceled));
    }
}
