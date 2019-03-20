using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphia.Common {
    /// <summary>value that is not a class field</summary>
    public class RemoteValue<LocalT,RemOperResT> : IReadWriteValue<LocalT> {
        public LocalT Value { get; private set; }

        private readonly LocalT _invalidValue;
        private readonly Func<RemOperResT,LocalT> _remToLocal;
        private readonly Func<LocalT,Task<RemOperResT>> _saveOperation;
        private readonly HashSet<string> _errors = new HashSet<string>();
        private int _pendingRemoteCalls = 0;
        private bool _remoteCallingEnabled = true;
        
        public IEnumerable<string> Errors => _errors;

        /// <summary>(oldValue, newValue) => shouldReplicate? </summary>
        public Func<LocalT,LocalT,bool> RemoteCallFilter { get; set; } = (_, __) => true;

        public event Validate<LocalT> Validate;
        public event ValueChangedRich<LocalT> Changed;
        
        public RemoteValue(
                LocalT initialValue, Func<LocalT, Task<RemOperResT>> saveOperation, 
                Func<RemOperResT,LocalT> remToLocal,
                Action<RemoteValue<LocalT,RemOperResT>> initialization = null

            //TODO due to bridge.net issue 2207 default(T) is wrongly compiled to 'null'. Revisit in future
        ) : this(initialValue, saveOperation, default(LocalT), remToLocal, initialization) {}

        public RemoteValue(
                LocalT initialValue, Func<LocalT, Task<RemOperResT>> saveOperation, LocalT invalidValue,
                Func<RemOperResT,LocalT> remToLocal, Action<RemoteValue<LocalT,RemOperResT>> initialization = null) {

            Value = initialValue;
		    
            _saveOperation = saveOperation;
            _invalidValue = invalidValue;
            _remToLocal = remToLocal;
            
            if (initialization != null) {
                _remoteCallingEnabled = false;
                try {
                    Logger.Debug(GetType(), "WithinConstructor initialization starting");
                    initialization(this);
                } finally {
                    _remoteCallingEnabled = true;
                    Logger.Debug(GetType(), "WithinConstructor initialization ended");
                }
            }
        }
        
        private void RecalculateErrors(LocalT input) {
            _errors.Clear();
            Validate?.Invoke(input, _errors);
        }
        
        public void Reset(bool isUserChange= false, object sender = null) {
            Initialize(_invalidValue, isUserChange, sender);
        }

        public void Initialize(LocalT newValue, bool isUserChange, object sender) {
            sender = sender ?? this;

            Logger.Debug(GetType(), "Initializing starting toValue={0} remoteCallingEnabled={1}", 
                newValue, _remoteCallingEnabled);

            var oldRemoteCallingEnabled = _remoteCallingEnabled;
            _remoteCallingEnabled = false;

            var oldValue = Value;
            var factSaved = newValue;

            RecalculateErrors(newValue); //may be rejected by validation BUT yet still errors should be calculated
            Value = newValue;
        
            Logger.Debug(GetType(), "DoChange OldValue={0} NewValue={1} FactSaved={2} Errors={3} Sender={4}", 
                oldValue, newValue, factSaved, Errors.PrettyToString(), sender);

            Changed?.Invoke(sender, oldValue, newValue, Errors, isUserChange);    
            _remoteCallingEnabled = oldRemoteCallingEnabled;
        }

        public async Task<Unit> DoChange(
                LocalT newValue, bool isUserChange, object sender = null, 
                bool mayBeRejectedByValidation = true) {
            
            return await DoChangeWithCallingSave(newValue, isUserChange, sender, mayBeRejectedByValidation);
        }
        
        private async Task<Unit> DoChangeWithCallingSave(
                LocalT newValue, bool isUserChange, object sender = null, 
                bool mayBeRejectedByValidation = true) {

            var oldValue = Value;
            var isSaveCalled = false;
            var factSaved = newValue;

            Logger.Debug(GetType(), "DoChange entering OldValue={0} NewValue={1} isUserChange={2} _pendingRemoteCalls={3}", 
                oldValue, newValue, isUserChange, _pendingRemoteCalls, _remoteCallingEnabled);
            
            _pendingRemoteCalls++;
            RecalculateErrors(newValue); //may be rejected by validation BUT yet still errors should be calculated

            var shouldReplicateFilterAnswer = RemoteCallFilter(Value, newValue);

            Logger.Debug(GetType(), "DoChange errorsCount={0} RemoteCallingEnabled={1} mayBeRejectedByValidation={2} shouldReplicateFilterAnswer={3}", 
                _errors.Count, _remoteCallingEnabled, mayBeRejectedByValidation, shouldReplicateFilterAnswer);

            if (!mayBeRejectedByValidation || _errors.Count <= 0) {
                try {
                    if (_remoteCallingEnabled && shouldReplicateFilterAnswer) {
                        isSaveCalled = true;
                        var raw = await _saveOperation(newValue);
                        factSaved = _remToLocal(raw);
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
                var factNewValue = factSaved;
		    
                shouldPropagate = object.Equals(factNewValue, newValue);
            }
            
            Logger.Debug(GetType(), "DoChange OldValue={0} NewValue={1} FactSaved={2} Errors={3} Sender={4} shouldPropagate?={5} isSaveCalled={6} _pendingRemoteCalls={7}", 
                oldValue, newValue, factSaved, Errors.PrettyToString(), sender, shouldPropagate, isSaveCalled, _pendingRemoteCalls);

            _pendingRemoteCalls--;

            if (shouldPropagate && _pendingRemoteCalls <= 0) {
                Changed?.Invoke(sender, oldValue, newValue, Errors, isUserChange);    
            }
            
            return Unit.Instance;
        }
        
        public override string ToString() {
            return string.Format("[RemoteValue: Value={0}, Errors={1}, Validators?={2} Changed?={3}]", Value, Errors.PrettyToString(), Validate!=null, Changed != null);
        }
    }
}
