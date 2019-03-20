using System.Collections.Generic;

namespace Philadelphia.Common {
    public interface IRestrictedMultipleReadWriteValueView<WidgetT,LocalCollT> : IReadWriteValueView<WidgetT,LocalCollT> {
        LocalCollT PermittedValues {set; }
    }
}
