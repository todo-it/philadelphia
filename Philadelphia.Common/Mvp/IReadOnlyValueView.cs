using System.Collections.Generic;

namespace Philadelphia.Common {
    public delegate void UiErrorsUpdated(object sender, ISet<string> errors);

    public interface IReadOnlyValueView<WidgetT,ValueT> : IView<WidgetT> {
        event UiErrorsUpdated ErrorsChanged;

        /// <summary>
        /// doesn't cause IReadWriteValueView->Changed to be raised
        /// </summary>
        ValueT Value { get; set; }
        ISet<string> Errors { get; }
        void SetErrors(ISet<string> errors, bool causedByUser); //includes validation,conversion and remote save errors
    }
}
