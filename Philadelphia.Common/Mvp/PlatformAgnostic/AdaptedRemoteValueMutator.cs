using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Philadelphia.Common {
    public class AdaptedRemoteValueMutator<ValueT,OperT> : IReadWriteValue<ValueT> {
        private ValueT _oldValue;
        private readonly Func<ValueT,Task<OperT>> _saveOperation;
        private readonly Func<ValueT> _getValue;
        private readonly Action<ValueT> _setValue;
        private bool _doChangeInvoked;
        private readonly HashSet<string> _errors = new HashSet<string>();
	    
        public IEnumerable<string> Errors => _errors;
        public bool RemoteCallingEnabled { get; set; } = true;
        public ValueT Value {
            get => _getValue();
            private set => _setValue(value);
        }

        public event Validate<ValueT> Validate;
        public event ValueChangedRich<ValueT> Changed;
        
        public AdaptedRemoteValueMutator(
            Func<ValueT> getValue, Action<ValueT> setValue, 
            Func<ValueT, Task<OperT>> saveOperation, 
            Action<AdaptedRemoteValueMutator<ValueT,OperT>> initialization = null) {

            _getValue = getValue;
            _setValue = setValue;
            _oldValue = getValue();
            _saveOperation = saveOperation;
			
            if (initialization != null) {
                RemoteCallingEnabled = false;
                initialization(this);
                RemoteCallingEnabled = true;
            }
        }
        
        private void RecalculateErrors(ValueT input) {
            _errors.Clear();
            Validate?.Invoke(input, _errors);
        }

        public void Reset(bool isUserChange, object sender) {
            sender = sender ?? this;
            var newValue = _getValue();
            var oldValue = _oldValue;
            _oldValue = newValue;

            RecalculateErrors(newValue); //may be rejected by validation BUT yet still errors hould be calculated
            Value = newValue;
            
            Logger.Debug(GetType(), "AdaptedRemoteValueMutator->Reset() OldValue={0} NewValue={1} Errors={2} Sender={3}", 
                oldValue, newValue, Errors.PrettyToString(), sender);

            Changed?.Invoke(sender, oldValue, newValue, Errors, isUserChange);
        }

        public async Task<Unit> DoChange(ValueT newValue, bool isUserChange, object sender = null, bool mayBeRejectedByValidation = true) {
            if (_doChangeInvoked) {
                Logger.Debug(GetType(), "Skipping DoChange({0}) to skip infinite cycle: model change<->view change", newValue);
                return Unit.Instance;
            }

            _doChangeInvoked = true;

            try {
                return await DoChangeWithSave(newValue, isUserChange, sender, mayBeRejectedByValidation);
            } finally {
                _doChangeInvoked = false;
            }
        }

        private async Task<Unit> DoChangeWithSave(ValueT newValue, bool isUserChange, object sender = null, bool mayBeRejectedByValidation = true) {
            var oldValue = _oldValue;
            _oldValue = newValue;

            RecalculateErrors(newValue); //may be rejected by validation BUT yet still errors hould be calculated

            if (!mayBeRejectedByValidation || _errors.Count <= 0) {
                try {
                    if (RemoteCallingEnabled) {
                        await _saveOperation(newValue);    
                    }
					
                    Value = newValue;
                } catch (Exception ex) {
                    Logger.Error(GetType(), "Remote save failed due to {0}", ex);
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
        
        public async Task InitializeInitialValue(ValueT value) {
            RemoteCallingEnabled = false;
            await DoChange(value, false, this, false);
            RemoteCallingEnabled = true;
        }

        public override string ToString() {
            return string.Format("[AdaptedRemoteValue: Value={0}, Errors={1}, Validators?={2} Changed?={3}]", Value, Errors.PrettyToString(), Validate!=null, Changed != null);
        }
    }
}
