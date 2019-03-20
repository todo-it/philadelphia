using System;
using System.Collections.Generic;
using System.Linq;

namespace Philadelphia.Common {
    public static class ActionModelExtensions {
        public static ValueChangedRich<bool> BindEnableAndInitialize<T>(this IActionModel<T> model, IReadOnlyValue<bool> validator) {
            void Handler(object sender, bool oldValue, bool newValue, IEnumerable<string> errors, bool isUserAction) {
                model.ChangeEnabled(newValue, errors, isUserAction);
                Logger.Debug(typeof(ActionModelExtensions), "IActionModelExtensions->BindEnableAndInitialize set enable to {0}", newValue);
            }

            validator.Changed += Handler;
            Handler(validator, validator.Value, validator.Value, validator.Errors, false);
            return Handler;
        }

        public static ValueChangedRich<bool> BindEnableAndInitializeAsObserving<T>(
                this IActionModel<T> model, Action<AggregatedErrorsValue<bool>> observes) {

            var isEnabled = new AggregatedErrorsValue<bool>(false, self => !self.Errors.Any(), observes);
            return BindEnableAndInitialize(model, isEnabled);
        }        
    }
}
