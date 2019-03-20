using System.Collections.Generic;
using System.Linq;
using Bridge.Html5;

namespace Philadelphia.Web {
    public class DataGridControllerDerivedGrouping<RecordT> : IRecordGrouping<RecordT> {
        public IEnumerable<GroupDescr> GroupRecords(
                IEnumerable<RecordT> allItems, IEnumerable<IDataGridColumn<RecordT>> colls) {
            
            return 
                colls.FirstOrDefault(x => x.IsGrouped)?.CurrentGrouping(allItems) ?? new List<GroupDescr>();
        }

        public IEnumerable<HTMLElement> BuildSummaryRow(
                IEnumerable<RecordT> allItems, GroupDescr grp, IEnumerable<IDataGridColumn<RecordT>> colls) {
            
            return 
                colls.Select(x => {
                    if (!x.IsGrouped && x.CurrentAggregation == null) {
                        return new HTMLSpanElement {TextContent = ""};
                    }
                    
                    if (x.IsGrouped) {
                        return new HTMLSpanElement {TextContent = grp.UserFriendlyGroupName};
                    }
                    
                    var result = x.CurrentAggregation(
                        allItems
                            .Skip(grp.FromPhsIdx)
                            .Take(1+grp.TillPhsIdx - grp.FromPhsIdx) );
                    
                    return new HTMLSpanElement {TextContent = result};
                });
        }
    }
}
