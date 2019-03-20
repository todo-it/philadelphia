using System;
using System.Collections.Generic;
using System.Linq;

namespace Philadelphia.Common {
    public class PassToLinqOrderByCollector<RecordT> : IOrderByCollector<RecordT> {
        // REVIEW: could implement using only one mutable field of IEnumerable<RecordT>
        // this field would then be mutated by each call to AddOrderByRule
        // initially it wouldf be set to items
        private bool _firstRule = true;
        private IEnumerable<RecordT> _initially;
        private IOrderedEnumerable<RecordT> _later;

        public PassToLinqOrderByCollector(List<RecordT> items) {
            _initially = items;
        }

        public void AddOrderByRule<T>(Func<RecordT,T> key, IComparer<T> comparer) {
            if (_firstRule) {
                _firstRule = false;
                _later = _initially.OrderBy(key, comparer);
                _initially = null; //to avoid confusion
                return;
            }

            _later = _later.ThenBy(key, comparer);
        }

        public IEnumerable<RecordT> GetSortedResult() {
            return _firstRule ? _initially : _later;
        }
    }
}
