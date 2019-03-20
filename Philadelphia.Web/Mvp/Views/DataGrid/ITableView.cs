using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public interface ITableView : IView<HTMLElement> {
        SingleRowSelectionMode SingleClickMeaning {get; set; }
        bool Sortable { get; set; }
        bool Filterable { get; set; }
        bool Reloadable { get; set; }

        /// <summary>in pixels</summary>
        double? GetFactColumnWidth(int colIdx);
        
        int HeaderRowCount { get; }
        void DeleteHeaderRow(int position);
        void InsertHeader(IEnumerable<HeaderColumnDescr> items);
        
        int BodyRowCount { get; }
        void DeleteBodyRow(int position);
        void InsertBodyRow(int position, IEnumerable<Element> items);

        /// <summary>in pixels</summary>
        double? GetBodyRowHeight(int position);

        /// <summary>in pixels</summary>
        double? GetHeaderRowHeight();

        /// <summary>column index and sort order</summary>
        event Action<int,SortOrder> ColumnOrderChanged; //column index and user's desired sorting
        event Action Scrolled;
        event Action<RerenderReason> RerenderNeeded;
        
        event Action<int,RowSelectionMode> RowSelected;
        event Action<int> RowActivated;
        event Action<FilterRowsAction,string> FilterRows;

        event Action ReloadData;

        /// <summary> export listener is supposed to provide extraction Task to wait for </summary>
        event Action<MutableHolder<Task>> Export;

        /// <summary>for even/odd coloring</summary>
        int FirstRowIndex { set; }

        /// <summary>in pixels</summary>
        double LeadSpace { get; set; }

        /// <summary>in pixels</summary>
        double TrailSpace { get; set; }

        /// <summary>in pixels</summary>
        double ScrollPosition { get; set; }

        /// <summary>in pixels</summary>
        double BodyVisibleSpace { get; set; }

        /// <summary>in pixels</summary>
        double HeaderVisibleSpace { get; }

        IEnumerable<IView<HTMLElement>> Actions { get; }

        void SetColumnWidth(int colIdx, double newSizePx);
        void SetColumnOrder(int colIdx, SortOrder order);
        void RowCssClassAdd(int rowIndex, string className);
        void RowCssClassRemove(int rowIndex, string className);
    }
}
