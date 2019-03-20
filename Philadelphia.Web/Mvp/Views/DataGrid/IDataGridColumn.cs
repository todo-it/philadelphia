using System;
using System.Collections.Generic;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public interface IDataGridColumn<RecordT> {
        string Name { get; }
        Element CreateHeaderLabel();

        /// <summary>CreateColumnHeaderController needs to be called first</summary>
        Action<IWhereCollector<RecordT>> CurrentFilterImpl {get; }

        /// <summary>CreateColumnHeaderController needs to be called first</summary>
        Func<IEnumerable<RecordT>, string> CurrentAggregation { get; }

        /// <summary>CreateColumnHeaderController needs to be called first</summary>
        Func<IEnumerable<RecordT>, IEnumerable<GroupDescr>> CurrentGrouping { get; }

        /// <summary>header label may need to listen to model changes</summary>
        /// <param name="dgmodel"></param>
        void OnModelAttached(DataGridModel<RecordT> dgmodel);
        
        HTMLElement CreateColumnHeaderController(ITransformationMediator listener);
        
        string TextValueFor(RecordT item);
        void ExportValue(RecordT from, ICellValueExporter into);

        Element CreateAndInitViewFor(RecordT item);
        void DeleteViewFor(RecordT item);
        
        /// <summary>is currently grouped</summary>
        bool IsGrouped { get; }
        
        /// <summary>can user change order via UI?</summary>
        bool Orderable { get; }

        /// <summary>in pixels</summary>
        double Width { get; set; }

        /// <summary>in pixels</summary>
        double ComputedWidth {set; }

        void SortingRuleApply(IOrderByCollector<RecordT> collector, SortOrder sortBy);
        bool IsSelectionHandler {get; }

        /// <summary>in pixels</summary>
        double? MinimumWidth { get; set; }
    }
}
