using System.Collections.Generic;
using System.Threading.Tasks;

namespace Philadelphia.Common {
    public class LocalValue<T> : IReadWriteValue<T> {
        public T Value { get; private set;}
        private readonly T _invalidValue;
        private readonly HashSet<string> _errors = new HashSet<string>();
        public IEnumerable<string> Errors => _errors;

        public event Validate<T> Validate;
        public event ValueChangedRich<T> Changed;

        public LocalValue(T initialValue, T invalidValue= default(T)) {
            Value = initialValue;
            _invalidValue = invalidValue;
        }

        private void RecalculateErrors(T input) {
            _errors.Clear();
            Validate?.Invoke(input, _errors);
        }

        public void Reset(T toValue, bool isUserChange = false, object sender = null) {
            sender = sender ?? this;
            var newValue = toValue;
            var oldValue = Value;

            RecalculateErrors(newValue); //may be rejected by validation BUT yet still errors hould be calculated
            Value = newValue;

            Logger.Debug(GetType(), "LocalValue->Reset OldValue={0} NewValue={1} Errors={2} Sender={3}", 
                oldValue, newValue, Errors.PrettyToString(), sender);
            
            Changed?.Invoke(sender, oldValue, newValue, Errors, isUserChange);
        }

        public void Reset(bool isUserChange, object sender) {
            Reset(_invalidValue, isUserChange, sender);
        }

        public Task<Unit> DoChange(
                T newValue, bool isUserChange, object sender = null, bool mayBeRejectedByValidation = true) {

            var oldValue = Value;

            RecalculateErrors(newValue); //may be rejected by validation BUT yet still errors hould be calculated
			
            if (!mayBeRejectedByValidation || _errors.Count <= 0) {
                Value = newValue;
            }

            Logger.Debug(GetType(), "LocalValue->DoChange OldValue={0} NewValue={1} Errors={2} Sender={3}", oldValue, newValue, Errors.PrettyToString(), sender);

            if (sender == null) {
                sender = this;
            }
	
            Changed?.Invoke(sender, oldValue, newValue, Errors, isUserChange);		
            return Task.FromResult(Unit.Instance);
        }

        public override string ToString() {
            return string.Format("[SimpleValue: Value={0}, Errors={1}, Validators?={2} Changed?={3}]", Value, Errors.PrettyToString(), Validate!=null, Changed != null);
        }
    }
}
