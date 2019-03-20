using System;
using System.Collections.Generic;

namespace Philadelphia.Common {
    public static class ActionViewExtensions {
        // REVIEW: valuetuple
        public static Tuple<ValueChangedRich<bool>,Action> BindAction<WidgetT,T>(this IActionView<WidgetT> view, IActionModel<T> domain) {
            ValueChangedRich<bool> domainHandler = (sender, oldValue, newValue, errors, isUserAction) => {
                Logger.Debug(typeof(ActionViewExtensions), "BindAction: changing view to {0} and {1}", newValue, errors.PrettyToString());
                view.Enabled = newValue;
                view.DisabledReason = new HashSet<string>(errors);
            };
            domain.EnabledChanged += domainHandler;

            Action viewHandler = async () => {
                Logger.Debug(typeof(ActionViewExtensions), "BindAction: triggering enabled?={0} action {1} from view {2}", domain.Enabled, domain, view);
                if (!domain.Enabled) {
                    return;
                }
				
                view.State = ActionViewState.CreateOperationRunning();
                view.Enabled = false;
                view.DisabledReason = new HashSet<string>{I18n.Translate("Please wait while operation is running...")};
                try {
                    await domain.Trigger();
                    view.State = ActionViewState.CreateIdleOrSuccess();
                } catch(Exception ex) {
                    Logger.Error(typeof(ActionViewExtensions), "Bound action failed to execute {0}", ex);
                    view.State = ActionViewState.CreateOperationFailed(ex);
                    //should be handled by domain.ActionExecuted subscribers
                } finally {
                    view.Enabled = domain.Enabled;
                    view.DisabledReason = new HashSet<string>(domain.DisabledReasons);
                }
            };
            view.Triggered += viewHandler;
            return new Tuple<ValueChangedRich<bool>,Action>(domainHandler, viewHandler);
        }

        public static Tuple<ValueChangedRich<bool>,Action> BindActionAndInitialize<WidgetT,T>(this IActionView<WidgetT> view, IActionModel<T> domain) {
            view.OpensNewTab = domain.ResultInNewTab;
            var handlers = view.BindAction(domain);
            handlers.Item1(domain, domain.Enabled, domain.Enabled, domain.DisabledReasons, false);

            return handlers;
        }
    }
}
