using System;
using System.Collections.Generic;

namespace Philadelphia.Common {
    public interface IOrderByCollector<RecordT> {
        void AddOrderByRule<T>(Func<RecordT,T> key, IComparer<T> comparer);
    }
}
