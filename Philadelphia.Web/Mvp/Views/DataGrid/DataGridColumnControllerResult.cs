using System;
using System.Collections.Generic;

namespace Philadelphia.Web {
    public class DataGridColumnControllerResult<ModelT> {
        public Func<ModelT,bool> FilterImpl { get; set; }
        public Func<IEnumerable<ModelT>,string> AggregationImpl { get; set; }
        public Func<IEnumerable<ModelT>,IEnumerable<GroupDescr>> GroupingImpl { get; set; }
        public IComparer<ModelT> SortingImpl { get; set; }

        public Func<bool> IsGroupingActive {get; set; }
    }
}
