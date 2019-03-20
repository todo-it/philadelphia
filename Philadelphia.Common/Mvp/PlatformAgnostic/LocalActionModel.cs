using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Philadelphia.Common {
    public class LocalActionModel : IActionModel<Unit> {
        private readonly Func<Task> _localAction;
        private readonly ISet<string> _reasons = new HashSet<string>();

        public bool ResultInNewTab => false;
        public bool Enabled { get; private set;}
        public IEnumerable<string> DisabledReasons => _reasons;

        public event ValueChangedRich<bool> EnabledChanged;
        public event Action<ResultHolder<Unit>> ActionExecuted;

        public LocalActionModel(Action onSuccess = null, Func<Task> localAction = null) {
            _localAction = localAction;
            if (onSuccess != null) {
                ActionExecuted += x => {if (x.Success) { onSuccess();} };
            }
            Enabled = true;
        }
        
        public async Task<Unit> Trigger() {
            if (!Enabled) {
                Logger.Error(GetType(), "Not really running action because action is disabled");
                return Unit.Instance;
            }

            Logger.Debug(GetType(), "Triggering local action?={0}", _localAction != null);

            if (_localAction != null) {
                try {
                    await _localAction.Invoke();
                } catch(Exception ex) {
                    ActionExecuted?.Invoke(ResultHolder<Unit>.CreateFailure(ex.Message, ex));
                    throw;
                }
            }

            ActionExecuted?.Invoke(ResultHolder<Unit>.CreateSuccess(Unit.Instance));

            return Unit.Instance;
        }

        public void ChangeEnabled(bool newValue, IEnumerable<string> rawReasons, bool isUserAction) {
            var reasons = rawReasons.ToList();
            var oldValue = Enabled;
            Enabled = newValue;

            _reasons.Clear();
            reasons.ForEach(x => _reasons.Add(x));
			
            EnabledChanged?.Invoke(this, oldValue, Enabled, reasons, isUserAction);	
        }
    }
}
