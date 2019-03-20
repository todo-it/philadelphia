using System;
using System.Collections.Generic;

namespace Philadelphia.Web {
    public class AggregatorDef<T> {
        public string Label {get; }
        public Func<IEnumerable<T>,string> AggregatorFunc {get; }
        
        public AggregatorDef(string label, Func<IEnumerable<T>,string> aggregatorFunc) {
            Label = label;
            AggregatorFunc = aggregatorFunc;
        }
    }
}
