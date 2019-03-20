using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Philadelphia.Common {
    public class AdaptedLocalValue<T> : IReadWriteValue<T> {
        private readonly Func<T> _getValue;
        private readonly Action<T> _setValue;

        private readonly HashSet<string> _errors = new HashSet<string>();
        public IEnumerable<string> Errors => _errors;
        private T _oldValue;        
        private bool _doChangeInvoked;

        public T Value {
            get => _getValue();
            private set => _setValue(value);
        }

        public event ValueChangedRich<T> Changed;
        public event Validate<T> Validate;

        public AdaptedLocalValue(Func<T> getValue, Action<T> setValue) {
            _getValue = getValue;
            _setValue = setValue;
            _oldValue = getValue();
        }
        
        public void Reset(bool isUserChange, object sender) {
            sender = sender ?? this;
            var newValue = _getValue();
            var oldValue = _oldValue;
            _oldValue = newValue;

            RecalculateErrors(newValue); //may be rejected by validation BUT yet still errors hould be calculated
            Value = newValue;
            
            Logger.Debug(GetType(), "AdaptedLocalValue->Reset OldValue={0} NewValue={1} Errors={2} Sender={3}", 
                oldValue, newValue, Errors.PrettyToString(), sender);

            Changed?.Invoke(sender, oldValue, newValue, Errors, isUserChange);
        }
        
        private void RecalculateErrors(T input) {
            _errors.Clear();
            Validate?.Invoke(input, _errors);
        }

        public Task<Unit> DoChange(T newValue, bool isUserChange, object sender = null, bool mayBeRejectedByValidation = true) {
            if (_doChangeInvoked) {
                Logger.Debug(GetType(), "Skipping DoChange({0}) to skip infinite cycle: model change<->view change", newValue);
                return Task.FromResult(Unit.Instance);
            }

            _doChangeInvoked = true;

            try {
                return DoChangeAsync(newValue, isUserChange, sender, mayBeRejectedByValidation);
            } finally {
                _doChangeInvoked = false;
            }
        }
       
        private Task<Unit> DoChangeAsync(
                T newValue, bool isUserChange, object sender = null, bool mayBeRejectedByValidation = true) {

            var oldValue = _oldValue;
            _oldValue = newValue;

            RecalculateErrors(newValue); //may be rejected by validation BUT yet still errors hould be calculated
			
            if (!mayBeRejectedByValidation || _errors.Count <= 0) {
                Value = newValue;
            }

            Logger.Debug(GetType(), "DelegatingValue->DoChange OldValue={0} NewValue={1} Errors={2} Sender={3}", oldValue, newValue, Errors.PrettyToString(), sender);

            if (sender == null) {
                sender = this;
            }
	
            Changed?.Invoke(sender, oldValue, newValue, Errors, isUserChange);		
            return Task.FromResult(Unit.Instance);
        }
    }
}
