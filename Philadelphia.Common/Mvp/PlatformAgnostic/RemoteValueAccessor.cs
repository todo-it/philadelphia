using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Philadelphia.Common {
    public class RemoteValueAccessor<FullT,KeyT> : IByKeyValueAccessor<FullT,KeyT> {
        public KeyT Value { get; private set;}
        public FullT FullValue { get; private set; }

        private readonly KeyT _invalidValue;
        private readonly Func<KeyT, Task<FullT>> _getFullValueOperation;
        private readonly HashSet<string> _errors = new HashSet<string>();
        public IEnumerable<string> Errors => _errors;
        public bool RemoteCallingEnabled { get; set; } = true;

        public event Validate<KeyT> Validate;
        public event ValueChangedRich<KeyT> Changed;
        
        public RemoteValueAccessor(Func<KeyT, Task<FullT>> getFullValueOperation, 
            Action<RemoteValueAccessor<FullT,KeyT>> initializationOrNull = null
            //TODO due to bridge.net issue 2207 above two lines are wrongly compiled to 'null'. Revisit in future
        ) : this(getFullValueOperation, initializationOrNull, default(KeyT), default(KeyT)) {}

        public RemoteValueAccessor(Func<KeyT, Task<FullT>> getFullValueOperation, 
                Action<RemoteValueAccessor<FullT,KeyT>> initializationOrNull, 
                KeyT initialValue,
                KeyT invalidValue) {

            Value = initialValue;
            _getFullValueOperation = getFullValueOperation;
            _invalidValue = invalidValue;

            if (initializationOrNull != null) {
                RemoteCallingEnabled = false;
                initializationOrNull(this);
                RemoteCallingEnabled = true;
            }
        }
        
        private void RecalculateErrors(KeyT input) {
            _errors.Clear();
            Validate?.Invoke(input, _errors);
        }
        
        public void Reset(bool isUserChange, object sender) {
            var old = RemoteCallingEnabled;
            RemoteCallingEnabled = false;
            try {
                InitializeInitialValue(_invalidValue, isUserChange, sender);
            } finally {
                RemoteCallingEnabled = old;
            }
        }
        
        public void InitializeInitialValue(KeyT value, bool isUserChange = false, object sender = null) {
            var oldRemoteCallingEnabled = RemoteCallingEnabled;
            RemoteCallingEnabled = false;
            
            sender = sender ?? this;
            var newValue = value;
            var oldValue = Value;

            RecalculateErrors(newValue); //may be rejected by validation BUT yet still errors hould be calculated
            Value = newValue;

            Logger.Debug(GetType(), "OldValue={0} NewValue={1} Errors={2} Sender={3}", 
                oldValue, newValue, Errors.PrettyToString(), sender);
            
            Changed?.Invoke(sender, oldValue, newValue, Errors, isUserChange);
            
            RemoteCallingEnabled = oldRemoteCallingEnabled;
        }

        public Task<Unit> DoChange(
                KeyT newValue, bool isUserChange, object sender = null, 
                bool mayBeRejectedByValidation = true) {

            return DoChangeWithSave(newValue, isUserChange, sender, mayBeRejectedByValidation);
        }

        public async Task<Unit> DoChangeWithSave(
                KeyT newValue, bool isUserChange, object sender = null, 
                bool mayBeRejectedByValidation = true) {

            var oldValue = Value;

            RecalculateErrors(newValue); //may be rejected by validation BUT yet still errors hould be calculated

            if (!mayBeRejectedByValidation || _errors.Count <= 0) {
                try {
                    if (RemoteCallingEnabled) {
                        FullValue = await _getFullValueOperation(newValue);
                    }
                    Value = newValue;
                } catch (Exception ex) {
                    Logger.Error(GetType(), "Get internal value failed due to {0}", ex);
                    _errors.Add(ex.Message);
                }
            }

            Logger.Debug(GetType(), "OldValue={0} NewValue={1} Errors={2} Sender={3}", oldValue, newValue, Errors.PrettyToString(), sender);

            if (sender == null) {
                sender = this;
            }
            
            Changed?.Invoke(sender, oldValue, newValue, Errors, isUserChange);
            return Unit.Instance;
        }
        
        public override string ToString() {
            return string.Format("[RemoteValueAccessor: Value={0}, Errors={1}, Validators?={2} Changed?={3}]", Value, Errors.PrettyToString(), Validate!=null, Changed != null);
        }
    }
}
