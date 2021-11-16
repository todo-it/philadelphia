using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Philadelphia.Common;
using Bridge.Html5;

namespace Philadelphia.Web {
    public delegate double? CalculateTbodyHeight(HTMLElement tableContainer, double tableHeaderHeight, double? prototypeRowHeight);

    public class DataGridModel<RecordT> {
        private static readonly char[] WhiteSpaceToSplitOn = {' ', '\t'};
        private readonly FilterableSortableObservableCollection<RecordT> _items;
        
        public Action ReloadDataActionOrNull {get; }
        public IObservableCollection<RecordT> Selected { get; } = new FilterableSortableObservableCollection<RecordT>();
        public IObservableCollection<RecordT> Items => _items;
        public IObservableCollection<IDataGridColumn<RecordT>> Columns { get; }
        public IReadWriteValue<RecordT> Activated = new LocalValue<RecordT>(default(RecordT));
        public CalculateTbodyHeight TbodyHeightProvider { get; }
        public Func<RecordT,string> CustomRowCssClassOrNull { get; set;}
        
        public IRecordGrouping<RecordT> CurrentGroupingLogic {get; } 
            = new DataGridControllerDerivedGrouping<RecordT>();
        
        private DataGridModel(
                Action reloadDataAction, CalculateTbodyHeight tbodyHeight, 
                IEnumerable<IDataGridColumn<RecordT>> initialColumns) {

            ReloadDataActionOrNull = reloadDataAction;
            TbodyHeightProvider = tbodyHeight;
            _items = new FilterableSortableObservableCollection<RecordT>();
            Columns = new FilterableSortableObservableCollection<IDataGridColumn<RecordT>>(initialColumns);
        }
        
        /// <summary> probably good idea to better use 'params' version for shorter syntax in client's code</summary>
        public static (DataGridModel<RecordT> model,DataGridModelPresenter<RecordT> presenter) CreateAndBindReloadable(
                ITableView view, Action reloadDataAction, CalculateTbodyHeight tbodyHeight, 
                IEnumerable<IDataGridColumn<RecordT>> initialColumns) {

            var model = new DataGridModel<RecordT>(reloadDataAction, tbodyHeight, initialColumns);
            var presenter = model.BindAndInitialize(view);
            return (model, presenter);
        }

        public static (DataGridModel<RecordT> model, DataGridModelPresenter<RecordT> presenter) CreateAndBindReloadable(
            ITableView view, Action reloadDataAction, CalculateTbodyHeight tbodyHeight,
            params IDataGridColumn<RecordT>[] initialColumns) {

            var model = new DataGridModel<RecordT>(reloadDataAction, tbodyHeight, initialColumns);
            var presenter = model.BindAndInitialize(view);
            return (model, presenter);
        }

        /// <summary> probably good idea to better use 'params' version for shorter syntax in client's code</summary>
        public static (DataGridModel<RecordT> model,DataGridModelPresenter<RecordT> presenter) CreateAndBindNonReloadable(
                ITableView view, CalculateTbodyHeight tbodyHeight, 
                IEnumerable<IDataGridColumn<RecordT>> initialColumns) {

            var model = new DataGridModel<RecordT>(null, tbodyHeight, initialColumns);
            var presenter = model.BindAndInitialize(view);
            return (model, presenter);
        }
        
        public static (DataGridModel<RecordT> model,DataGridModelPresenter<RecordT> presenter) CreateAndBindNonReloadable(
                ITableView view, CalculateTbodyHeight tbodyHeight, 
                params IDataGridColumn<RecordT>[] initialColumns) {

            var model = new DataGridModel<RecordT>(null, tbodyHeight, initialColumns);
            var presenter = model.BindAndInitialize(view);
            return (model, presenter);
        }

        public void ChangeGlobalFilter(Action<IWhereCollector<RecordT>> collect) {
            _items.ChangeItemsFilter(collect, 0);
        }

        public void ChangeColumnFilter(Action<IWhereCollector<RecordT>> collect) {
            _items.ChangeItemsFilter(collect, 1);
        }
        
        public void ChangeItemsSorting(Action<IOrderByCollector<RecordT>> collect) {
            _items.ChangeItemsSorting(collect);
        }
        
        public DatagridContent ExportDataToJson() {
            var exporter = new CellValueExporter();
            var labels = new List<string>();
            var rows = new List<List<DatagridCell>>(Items.Length);
            
            Columns.ForEach(col => labels.Add(col.Name));

            var f = DateTime.UtcNow;

            Items.ForEach(itm => {
                var newRow = new List<DatagridCell>(Columns.Length);
                rows.Add(newRow);
                exporter.SetTarget(newRow);
                Columns.ForEachI((iCol,col) => col.ExportValue(itm, exporter));
            });

            var t = DateTime.UtcNow;
            Logger.Debug(GetType(), "Exported {0} rows in {1} seconds. {2} ms per row",
                Items.Length, (t-f).TotalSeconds, ((t-f).TotalSeconds*1000)/Items.Length);

            var result = new DatagridContent {
                labels = labels.ToArray(),
                rows = new DatagridCell[rows.Count][]
            };
            rows.ForEachI((iRow,row) => result.rows[iRow] = row.ToArray());

            return result;
        }

        //single column match
        public void ChangeGlobalFilter(FilterRowsAction type, Func<IDataGridColumn<RecordT>,RecordT,bool> matches) {
            ChangeGlobalFilter(collector => {
                switch (type) {
                    case FilterRowsAction.Remove:
                        break;
                    case FilterRowsAction.Change:
                        collector.AddWhereRule(
                            record => Columns.FirstOrDefault(col => matches(col, record)),
                            found => found != null
                        );
                        break;
                    default:
                        Logger.Error(GetType(), "Unknown FilterRowsAction in OnGlobalFilterChanged()");
                        throw new Exception("Unknown FilterRowsAction");
                }
            });
        }

        //multi column match
        public void ChangeGlobalFilter(
                FilterRowsAction type, Func<IRandomAccessCollection<IDataGridColumn<RecordT>>,RecordT,bool> matches) {
            
            ChangeGlobalFilter(collector => {
                switch (type) {
                    case FilterRowsAction.Remove:
                        break;
                    case FilterRowsAction.Change:
                        collector.AddWhereRule(
                            record => matches(Columns, record),
                            found => found
                        );
                        break;
                    default:
                        Logger.Error(GetType(), "Unknown FilterRowsAction in OnGlobalFilterChanged()");
                        throw new Exception("Unknown FilterRowsAction");
                }
            });
        }

        public void ChangeGlobalFilterToContainsCaseInsensitive(FilterRowsAction type, string newValueRaw) {
            var newValue = newValueRaw.ToLower();

            ChangeGlobalFilter(type, 
                (col,record) => (col.TextValueFor(record) ?? "").ToLower().Contains(newValue) );
        }
        
        public void ChangeGlobalFilterToBeginsWithOrAnyWordBeginsCaseInsensitive(FilterRowsAction type, string newValueRaw) {
            var newValue = newValueRaw.ToLower();

            ChangeGlobalFilter(type, 
                (col,record) => {
                    var itm = (col.TextValueFor(record) ?? "").ToLower();

                    return 
                        itm.StartsWith(newValue) || 
                        itm.Split(WhiteSpaceToSplitOn)
                            .Any(wrd => wrd.StartsWith(newValue));
            });
        }

        public void ChangeGlobalFilterToAllPhrasesContainedInSomeColumnCaseInsensitive(FilterRowsAction type, string userFilterInput) {
            var phrases = new Regex(@"\w+")
                .Matches(userFilterInput.ToLower())
                .Cast<Match>()
                .Where(x => x.Success)
                .Select(x => x.Value)
                .ToArray();
                
            ChangeGlobalFilter(type,
                (richCols, record) => {
                    var cols = richCols
                        .Select(col => (col.TextValueFor(record) ?? "")
                        .ToLower())
                        .ToArray();
                    
                    return phrases.All(phrase => 
                        cols.Any(colTxt => colTxt.Contains(phrase)));
                });
        }
    }
}
