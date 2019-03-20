using System;
using System.Collections.Generic;
using System.Linq;

namespace Philadelphia.Common {
    public class PassToLinqWhereCollector<RecordT> : IWhereCollector<RecordT> {
        private IEnumerable<RecordT> _items;

        public PassToLinqWhereCollector(IEnumerable<RecordT> items) => _items = items;

        public void AddWhereRule<T>(Func<RecordT,T> key, Func<T,bool> shouldStayInSet) => 
            _items = _items.Where(x => shouldStayInSet(key(x)));

        public IEnumerable<RecordT> GetFilteredResult() => _items;
    }
}
