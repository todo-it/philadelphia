using System;
using System.Collections.Generic;

namespace Philadelphia.Web {
    public class GrouperDef<T> {
        public string Label {get; }
        public Func<IEnumerable<T>,IEnumerable<GroupDescr>> GroupingFunc {get; }
        
        public GrouperDef(string label, Func<IEnumerable<T>,IEnumerable<GroupDescr>> groupingFunc) {
            Label = label;
            GroupingFunc = groupingFunc;
        }
    }
}
