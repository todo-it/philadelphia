using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    /// <summary>Runs long async operation. Can be remotely/on server but doesn't have to</summary>
    public class RemoteActionsCallerForm : 
            IOnShownNeedingForm<HTMLElement>, 
            IForm<HTMLElement, RemoteActionsCallerForm, RemoteActionsCallerForm.Outcome> {

        public enum Outcome {
            Succeeded,
            Canceled,
            Interrupted
        }
        private readonly List<Func<Task<Unit>>> _allOperations;
        private List<Func<Task<Unit>>> _operations;
        private int _operationNo;
        private int _allOperationsCount;
        private readonly RemoteActionsCallerFormView _view;
        private bool _endedInvoked;
        private readonly bool _needsNewTab;
        private int Percentage => (int)(100.0m*_operationNo / _allOperationsCount);

        public event Action<RemoteActionsCallerForm,Outcome> Ended;
        public string Title => I18n.Translate("Waiting for server");
        public IFormView<HTMLElement> View => _view;
        private bool Interrupted {get; set;}
        private bool Erroneous {get; set;}
        
        public RemoteActionsCallerForm(Action<OperationBatchBuilder> operations, bool needsNewTab=false) : 
            this(new RemoteActionsCallerFormView(), new OperationBatchBuilder(operations).Build(), needsNewTab) {}

        public RemoteActionsCallerForm(RemoteActionsCallerFormView view, Action<OperationBatchBuilder> operations, bool needsNewTab=false) : 
            this(view, new OperationBatchBuilder(operations).Build(), needsNewTab) {}

        public RemoteActionsCallerForm(RemoteActionsCallerFormView view, IEnumerable<Func<Task<Unit>>> rawOperations, bool needsNewTab) {
            _view = view;
            _needsNewTab = needsNewTab;
            _allOperations = rawOperations.ToList();
            _allOperationsCount = _allOperations.Count;

            _view.Percentage.Value = Percentage.ToString();
        }

        public void OnShown() {
            _endedInvoked = false;
            Interrupted = false;
            Erroneous = false;
            
            _view.ErrorContainer.Value = ""; //reset former errors (if any)
            _operations = _allOperations.ToList();
            _allOperationsCount = _operations.Count;
            _operationNo = 0;
            _view.Percentage.Value = Percentage.ToString();
            
            RunNextOperation();
        }

        private void RunNextOperation() {
            _view.Percentage.Value = Percentage.ToString();
            _operationNo++;

            if (Interrupted) {
                if (!_endedInvoked) {
                    _endedInvoked = true;
                    Ended?.Invoke(this, Outcome.Interrupted);
                }
                
                return;
            }
            
            if (!_operations.Any()) {
                if (!_endedInvoked) {
                    _endedInvoked = true;
                    Ended?.Invoke(this, Outcome.Succeeded);
                }
                
                return;
            }
            var oper = _operations[0];
            _operations.RemoveAt(0);

            var act = new RemoteActionModel<Unit>(_needsNewTab, oper);
            Window.SetTimeout(async () => {
                Logger.Debug(GetType(), "starting operation no={0}", _operationNo-1);
                try {
                    await act.Trigger();
                } catch(Exception ex) {
                    Erroneous = true;
                    _view.ErrorContainer.Value = ex.Message;
                }
                
                Logger.Debug(GetType(), "ended operation no={0}", _operationNo-1);

                if (!Erroneous) {
                    RunNextOperation();    
                    return;
                }
                
                //Errorneous - user should cancel OR retry;
            }, 1);
        }
        
        public ExternalEventsHandlers ExternalEventsHandlers => 
            ExternalEventsHandlers.Create(() => {
                Interrupted = true;
                if (!_endedInvoked) {
                    _endedInvoked = true;
                    Ended?.Invoke(this, Outcome.Canceled);
                }
            });
    }
}
