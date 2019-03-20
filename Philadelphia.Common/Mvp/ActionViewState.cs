using System;

namespace Philadelphia.Common {
    public class ActionViewState {
        public ActionViewStateType Type { get; }

        public Exception ErrorOrNull { get; }

        private ActionViewState(ActionViewStateType type, Exception error) {
            Type = type;
            ErrorOrNull = error;
        }

        public static ActionViewState CreateIdleOrSuccess() {
            return new ActionViewState(ActionViewStateType.IdleOrSuccess, null);
        }
        
        public static ActionViewState CreateOperationRunning() {
            return new ActionViewState(ActionViewStateType.OperationRunning, null);
        }
        
        public static ActionViewState CreateOperationFailed(Exception error) {
            return new ActionViewState(ActionViewStateType.OperationFailed, error);
        }
    }
}
