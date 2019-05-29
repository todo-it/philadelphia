using System;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class ControlWithValueLogic<T> {
        private readonly Action<T,bool> _onChanged;
        private readonly Func<T> _getPhysicalValue;
        private readonly Action<T> _setPhysicalValue;
        private readonly Func<bool> _getEnabled;
        private readonly Action<bool> _setEnabled;
        private readonly Func<bool> _getIsValidating;
        private readonly Action<bool> _setIsValidating;
        private bool _replicateChange = true;
        private DateTime? _validation;
        private Optional<T> _lastCommittedValue = Optional<T>.CreateNone(); 
        private int _pendingValueChanges = 0;

        /// <summary>mainly to have sensible Tuple logging</summary>
        public Func<T,string> ValueToString { get; set; } = x => ""+x;

        public bool Enabled {
            get => _getEnabled();
            set {
                Logger.Debug(GetType(), "physical Enabled changing to {0}", value);
                _setEnabled(value);
            }
        }

        public bool IsValidating {
            get => _getIsValidating();
            set {
                Logger.Debug(GetType(), "physical Validating changing to {0}", value);
                _setIsValidating(value);
            }
        }

        public T Value {
            get => _getPhysicalValue();
            set {
                _replicateChange = false;
                
                try {
                    //to avoid any strange focus/flicker issues
                    var cur = _getPhysicalValue();

                    var physicalValueReallyChanged = 
                        value != null && !value.Equals(cur) || 
                        value == null && cur != null;

                    Logger.Debug(GetType(), "physical Value changed?={0}, fromValue={1} toValue={2} enabled={3} validationRunningSince={4} pendingValueChanges={5}", 
                        physicalValueReallyChanged, ValueToString(Value), ValueToString(value), Enabled, _validation, _pendingValueChanges);
                        
                    if (physicalValueReallyChanged && _pendingValueChanges <= 0) {
                        _setPhysicalValue(value);
                        _lastCommittedValue = value == null ? Optional<T>.CreateNone() : Optional<T>.CreateSome(value);
                    }
                } finally { 
                    _replicateChange = true;
                } 
            }
        }

        public int ValidationTriggerDelayMilisec {get; set; } = Magics.ValidationTriggerDelayMilisec;

        public ControlWithValueLogic(
                Action<T,bool> OnChanged,
                Func<T> getPhysicalValue, Action<T> setPhysicalValue,
                Func<bool> getEnabled, Action<bool> setEnabled,
                Func<bool> getIsValidating, Action<bool> setIsValidating) {
            
            _onChanged = OnChanged;
            _getPhysicalValue = getPhysicalValue;
            _setPhysicalValue = setPhysicalValue;
            _getEnabled = getEnabled;
            _setEnabled = setEnabled;
            _getIsValidating = getIsValidating;
            _setIsValidating = setIsValidating;
        }

        public void PhysicalChanged(bool dueToEnter, bool isUserGenerated) {
            var newValue = _getPhysicalValue();

            //isIrrelevant is used to trace last commited value so that change event after 
            //input event is not really causing _onChange() to be called. Thanks to that while
            //exiting changed field in RemoteValueMutator it doesn't cause another (not requested overwrite)
            //update call to server
            var isIrrelevant = 
                (!_lastCommittedValue.HasValue && _getPhysicalValue() == null ||
                _lastCommittedValue.HasValue && _lastCommittedValue.Value.Equals(newValue)) &&
                !dueToEnter;

            Logger.Debug(GetType(),"got onchange enabled?={0} replicateChange={1} newValue={2} isUserGenerated={3} _lastCommittedValue={4} isIrrelevant={5} _pendingValueChanges={6}", 
                Enabled, _replicateChange, ValueToString(newValue), isUserGenerated, 
                !_lastCommittedValue.HasValue ? null : ValueToString(_lastCommittedValue.Value), 
                isIrrelevant, _pendingValueChanges);

            if (!Enabled || !_replicateChange || isIrrelevant) {
                return;
            }
            
            var myValidation = DateTime.UtcNow;
            _validation = myValidation;
            _setIsValidating(true);
            _lastCommittedValue = newValue == null ? Optional<T>.CreateNone() : Optional<T>.CreateSome(newValue);
            _pendingValueChanges++;

            Window.SetTimeout(() => {
                    if (myValidation != _validation) {
                        _pendingValueChanges--;
                        Logger.Debug(GetType(), 
                            "Validation discarded {0} because it is not the most recent one: {1} ", 
                            myValidation, _validation);

                        return;
                    }
                    
                    var factNewValue = _getPhysicalValue();
                    var sameValue = Common.ObjectExtensions.IsSameAs(factNewValue, newValue);
                    Logger.Debug(GetType(), "Validation {0} orgNewValue={1} factNewValue={2} areSame={3}", 
                        myValidation, newValue, factNewValue, sameValue);

                    // don't inform domain that new value was typed if user didn't finish typing OR value was mutated programatically
                    if (!sameValue) {
                        _pendingValueChanges--;
                        Logger.Debug(GetType(), "Validation discarded because values are not the same");
                        return;
                    }

                    _validation = null; //consumed
                    _setIsValidating(false);
                    NotifyAboutUserChange(isUserGenerated);
                    _pendingValueChanges--;
                }, 
                ValidationTriggerDelayMilisec);
        }
        
        private void NotifyAboutUserChange(bool isUserGenerated) {
            Logger.Debug(GetType(),"ControlWithValueLogic user?={0} changed value to {1} replicateChange?={2} _pendingValueChanges={3}", 
                isUserGenerated, _getPhysicalValue(), _replicateChange, _pendingValueChanges);

            if (!_replicateChange) {
                return;
            }
            
            _onChanged(_getPhysicalValue(), isUserGenerated);
        }

        /// <summary>useful f.e. for SELECT items replace</summary>
        public void PreserveValueDuringOperation(Action action) {
            _replicateChange = false;
            var value = _getPhysicalValue();
            try {
                action();
            } finally {
                _replicateChange = true;
            }
            _setPhysicalValue(value);
        }
    }
}
