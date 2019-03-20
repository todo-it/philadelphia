using System;

namespace Philadelphia.Web {
    public class RowFilter<T> {
        public Func<T,bool> Filter { get; }
        public string UserFriendlyReason { get; }

        public RowFilter(Func<T,bool> filter, string reason) {
            Filter = filter;
            UserFriendlyReason = reason;
        }
    }
}
