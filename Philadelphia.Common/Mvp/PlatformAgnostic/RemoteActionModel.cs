using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Philadelphia.Common {
    public class RemoteActionModel<T> : IActionModel<T> {
        private readonly ISet<string> _reasons = new HashSet<string>();
        private readonly Func<Task<T>> _remoteAction;
        private volatile bool _isExecuting;
        private readonly List<Action> _pending = new List<Action>();
        
        public bool ResultInNewTab { get; }

        public bool Enabled { get; private set;}
        public IEnumerable<string> DisabledReasons => _reasons;

        public event ValueChangedRich<bool> EnabledChanged;
        public event Action<ResultHolder<T>> ActionExecuted;

        public RemoteActionModel(bool needsNewTab, Func<Task<T>> remoteAction, Action<T> onSuccess = null, Action<ResultHolder<T>> onFailure = null) {
            Enabled = true;
            ResultInNewTab = needsNewTab;
            _remoteAction = remoteAction;
            if (onSuccess != null) {
                ActionExecuted += x => {if (x.Success) { onSuccess(x.Result);} };
            }
            if (onFailure != null) {
                ActionExecuted += x => {if (!x.Success) { onFailure(x);} };
            }
        }

        public RemoteActionModel(bool needsNewTab, Func<Task<T>> remoteAction, Action<T> onSuccess = null) : this(needsNewTab, remoteAction, onSuccess, null) {
        }

        public RemoteActionModel(bool needsNewTab, Func<Task<T>> remoteAction) : this(needsNewTab, remoteAction, null, null) {
        }
        
        public RemoteActionModel(Func<Task<T>> remoteAction, Action<T> onSuccess = null, Action<ResultHolder<T>> onFailure = null) : this(false, remoteAction, onSuccess, onFailure) {
        }

        public RemoteActionModel(Func<Task<T>> remoteAction, Action<T> onSuccess = null) : this(false, remoteAction, onSuccess, null) {
        }

        public RemoteActionModel(Func<Task<T>> remoteAction) : this(false, remoteAction, null, null) {
        }

        private void StartedExecution() => _isExecuting = true;

        private void EndedExecution() {
            _isExecuting = false;

            var pendingCopy = _pending.ToList();
            _pending.Clear();
            pendingCopy.ForEach(x => x());
        }
        
        private void ExecOrPostpone(Action toExec) {
            if (!_isExecuting) {
                toExec();
                return;
            }
            _pending.Add(toExec);
        }

        public async Task<T> Trigger() {
            if (!Enabled) {
                Logger.Error(GetType(), "Not really running remote action because action is disabled");
                throw new Exception("trying to trigger disabled action");
            }

            Logger.Debug(GetType(), "Running remote action");
            ResultHolder<T> resultAsHolder;

            try {
                StartedExecution();
                var result = await _remoteAction();
                resultAsHolder = ResultHolder<T>.CreateSuccess(result);
                await ExecOnUiThread.Exec(() => ActionExecuted?.Invoke(resultAsHolder));
                    
                return result;
            } catch (Exception ex) {
                resultAsHolder = ResultHolder<T>.CreateFailure(ex.Message, ex);
                await ExecOnUiThread.Exec(() => ActionExecuted?.Invoke(resultAsHolder));

                throw;
            } finally {
                Logger.Debug(GetType(), "Remote action ended");
                EndedExecution();
            }
        }
        
        public void ChangeEnabled(bool newValue, IEnumerable<string> rawReasons, bool isUserAction) {
            var reasons = rawReasons.ToList();
            var oldValue = Enabled;
            Enabled = newValue;

            _reasons.Clear();
            reasons.ForEach(x => _reasons.Add(x));
			
            ExecOrPostpone(() => EnabledChanged?.Invoke(this, oldValue, Enabled, reasons, isUserAction));
        }
    }
}
