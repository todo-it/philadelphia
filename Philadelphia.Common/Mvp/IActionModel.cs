using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Philadelphia.Common {
    public interface IActionModel<T> {
        bool ResultInNewTab {get; }
        bool Enabled { get; }
        IEnumerable<string> DisabledReasons { get; } 

        Task<T> Trigger();
        void ChangeEnabled(bool newValue, IEnumerable<string> reasons, bool isUserAction);

        event ValueChangedRich<bool> EnabledChanged;
        event Action<ResultHolder<T>> ActionExecuted;
    }
}
