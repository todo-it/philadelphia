using System;

namespace Philadelphia.Web {
    public class FilterDef<T> {
        public string Label {get; }
        public Func<T,T,bool> FilterFunc {get; }

        public FilterDef(string label, Func<T,T,bool> filterParamTestedValue) {
            Label = label;
            FilterFunc = filterParamTestedValue;
        }
    }
}
