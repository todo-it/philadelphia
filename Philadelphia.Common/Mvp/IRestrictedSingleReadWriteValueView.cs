using System.Collections.Generic;

namespace Philadelphia.Common {
    public interface IRestrictedSingleReadWriteValueView<WidgetT,ValueT> : IReadWriteValueView<WidgetT,ValueT> {
        IEnumerable<ValueT> PermittedValues {set; }
    }
}
