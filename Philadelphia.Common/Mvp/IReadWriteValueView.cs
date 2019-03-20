using System.Collections.Generic;

namespace Philadelphia.Common {
    public delegate void ValueChangedSimple<in T>(T newValue, bool isUserInput);

    public interface IReadWriteValueView<WidgetT,ValueT> : IReadOnlyValueView<WidgetT,ValueT>,IToggleableEnablement {
        /// <summary>
        /// is not raised on programmatic value overwrite
        /// </summary>
        event ValueChangedSimple<ValueT> Changed;
        bool IsValidating { set; }
        ISet<string> DisabledReasons { set; } 
    }
}
