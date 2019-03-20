using System.Collections.Generic;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    /// <summary>
    /// meaning of bool "is data correct" - true is no error, false is has error
    /// </summary>
    public class ValidationIndicatorView : IReadOnlyValueView<HTMLElement,bool> {
        private readonly HTMLElement _widget;

        public HTMLElement Widget => _widget;
        public event UiErrorsUpdated ErrorsChanged;

        public bool Value {
            get => _widget.ClassList.Contains(Magics.CssClassFailed);
            set => _widget.AddOrRemoveClass(!value, Magics.CssClassFailed);
        }

        public ValidationIndicatorView() {
            _widget = new HTMLDivElement {
                Id = UniqueIdGenerator.GenerateAsString(),
                ClassName = GetType().FullName
            };
            Value = true; //success
        }

        public ISet<string> Errors => new HashSet<string>();
        public void SetErrors(ISet<string> errors, bool causedByUser) {}
    }
}
