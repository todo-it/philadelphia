using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Philadelphia.Common {
    /// <summary>mutator for field contained in another class - uses expression to find mutated field name 'automagically'</summary>
    public class ClassFieldRemoteMutator<LocalT,RemT,ContT> : IReadWriteValue<LocalT> {
        private LocalT _value;
        public LocalT Value {
            get => _value;
            private set {
                _value = value; 
                if (_currentTarget != null) {
                    _setter(_currentTarget, _localToRemote(value));
                }
            }
        }
        
        private ContT _currentTarget;
        private readonly LocalT _invalidValue;
        private readonly Action<ContT, string> _postOperationConsumerOrNull;
        private readonly Func<LocalT, RemT> _localToRemote;
        private readonly Func<RemT, LocalT> _remoteToLocal;
        private readonly Func<RemT,Task<ContT>> _saveOperation;
        private readonly HashSet<string> _errors = new HashSet<string>();
        private readonly Func<object,object> _getter;
        private readonly Action<object,object> _setter;
        private readonly string _fieldName;
        private int _pendingRemoteCalls = 0;

        public IEnumerable<string> Errors => _errors;
        public bool RemoteCallingEnabled { get; set; } = true;

        public event Validate<LocalT> Validate;
        public event ValueChangedRich<LocalT> Changed;
        
        public ClassFieldRemoteMutator(Expression<Func<ContT,RemT>> getRemoteField,
                Func<LocalT,RemT> localToRemote,
                Func<RemT,LocalT> remoteToLocal,
                Func<RemT, Task<ContT>> saveOperation, 
                Action<ClassFieldRemoteMutator<LocalT,RemT,ContT>> initialization, 
                LocalT initialValue = default(LocalT),
                LocalT invalidValue = default(LocalT),
                Action<ContT,string> postOperationConsumerOrNull = null) {

            Value = initialValue;
            
            var prop = ExpressionUtil.ExtractField(getRemoteField) as PropertyInfo;
            if (prop == null) {
                throw new ArgumentException("getRemoteField's Member is not of expected 'PropertyInfo' type");
            }
            _getter = prop.GetValue;
            _setter = prop.SetValue;
            _fieldName = prop.Name;

            _localToRemote = localToRemote;
            _remoteToLocal = remoteToLocal;
            _saveOperation = saveOperation;
            _invalidValue = invalidValue;
            _postOperationConsumerOrNull = postOperationConsumerOrNull;

            if (initialization != null) {
                RemoteCallingEnabled = false;
                initialization(this);
                RemoteCallingEnabled = true;
            }
        }
        
        private RemT UseGetter(ContT inp) {
            return (RemT)_getter(inp);
        }

        public async Task BindTo(ContT inp) {
            _pendingRemoteCalls = 0; //reset

            _currentTarget = inp;
            var newVal = _remoteToLocal(UseGetter(inp));

            RemoteCallingEnabled = false;
            try {
                await DoChange(newVal, false, this, false);
            } finally {
                RemoteCallingEnabled = true;    
            }
        }

        private void RecalculateErrors(LocalT input) {
            _errors.Clear();
            Validate?.Invoke(input, _errors);
        }
        
        public void Reset(bool isUserChange, object sender) {            
            sender = sender ?? this;
            var newValue = _invalidValue;
            var oldValue = Value;
            
            RecalculateErrors(newValue); //may be rejected by validation BUT yet still errors should be calculated
            Value = newValue;

            Logger.Debug(GetType(), "DoChange OldValue={0} NewValue={1} Errors={2} Sender={3} _pendingRemoteCalls={4}", 
                oldValue, newValue, Errors.PrettyToString(), sender, _pendingRemoteCalls);

            Changed?.Invoke(sender, oldValue, newValue, Errors, isUserChange);
        }
        
        public async Task<Unit> DoChange(
                LocalT newValue, bool isUserChange, object sender = null, 
                bool mayBeRejectedByValidation = true) {

            return await DoChangeWithSave(newValue, isUserChange, sender, mayBeRejectedByValidation);
        }

        public async Task<Unit> DoChangeWithSave(
                LocalT newValue, bool isUserChange, object sender = null, 
                bool mayBeRejectedByValidation = true) {

            var oldValue = Value;
            var isSaveCalled = false;
            ContT factSaved = default(ContT);

            Logger.Debug(GetType(), "DoChange entering OldValue={0} NewValue={1} isUserChange={2} _pendingRemoteCalls={3}", 
                oldValue, newValue, isUserChange, _pendingRemoteCalls);
            
            _pendingRemoteCalls++;
            RecalculateErrors(newValue); //may be rejected by validation BUT yet still errors should be calculated

            if (!mayBeRejectedByValidation || _errors.Count <= 0) {
                try {
                    if (RemoteCallingEnabled) {
                        isSaveCalled = true;
                        factSaved = await _saveOperation(_localToRemote(newValue));
                        _postOperationConsumerOrNull?.Invoke(factSaved, _fieldName);
                    }
					
                    Value = newValue;
                } catch (Exception ex) {
                    Logger.Error(GetType(), "Remote save failed due to {0}", ex);
                    _errors.Add(ex.Message);
                }
            }

            if (sender == null) {
                sender = this;
            }

            /*
            Propagate change (to UI) if:
            -offline / without remote save because change is likely to be synchronous OR
            -online and current UI value is the same as before request

            Why? Because in UI user when he is in the middle of typing long value (that potentially is saved on-each-key-pressed)
            program should not overwrite UI value with domain value that is likely to be out of date (due to server saving network related delay).
            
            Thus don't change value in the field in the middle of typing if it is not neccessary
            */

            var shouldPropagate = true;

            //if operation failed then there's no value to be converted
            if (isSaveCalled && !_errors.Any()) {
                var factNewValue = _remoteToLocal(UseGetter(factSaved));
		    
                shouldPropagate = 
                    factNewValue == null && newValue == null ||
                    factNewValue != null && factNewValue.Equals(newValue);
            }
            
            Logger.Debug(GetType(), "DoChange OldValue={0} NewValue={1} Errors={2} Sender={3} shouldPropagate?={4} isSaveCalled={5} _pendingRemoteCalls={6}", 
                oldValue, newValue, Errors.PrettyToString(), sender, shouldPropagate, isSaveCalled, _pendingRemoteCalls);

            _pendingRemoteCalls--;

            if (shouldPropagate && _pendingRemoteCalls <= 0) {
                Changed?.Invoke(sender, oldValue, newValue, Errors, isUserChange);    
            }
            
            return Unit.Instance;
        }
        
        public async Task InitializeInitialValue(LocalT value) {
            RemoteCallingEnabled = false;
            await DoChange(value, false, this, false);
            RemoteCallingEnabled = true;
        }

        public override string ToString() {
            return string.Format("[RemoteValue: Value={0}, Errors={1}, Validators?={2} Changed?={3}]", Value, Errors.PrettyToString(), Validate!=null, Changed != null);
        }
    }
}
