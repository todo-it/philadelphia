using System;
using System.Collections.Generic;

namespace Philadelphia.Common {
    public interface IHasFixedPermittedValues<T> {
        // TODO: named value tuple
        /// <summary>internal value and label</summary>
        IEnumerable<Tuple<T,string>> PermittedValues {set; }
    }
}
