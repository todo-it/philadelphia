using System.Collections.Generic;
using Bridge.Html5;

namespace Philadelphia.Web {
    public interface IRecordGrouping<T> {
        IEnumerable<GroupDescr> GroupRecords(IEnumerable<T> input, IEnumerable<IDataGridColumn<T>> colls);
        IEnumerable<HTMLElement> BuildSummaryRow(IEnumerable<T> allItems, GroupDescr inp, IEnumerable<IDataGridColumn<T>> colls);
    }
}
