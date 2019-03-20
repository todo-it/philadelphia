using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class DataGridModelPresenter<RecordT> {
        private readonly DataGridModel<RecordT> _model; 
        private readonly ITableView _view;
        
        private bool _disableSelectionChangedPropagation;
        private double? _rowHeightPx;
        private bool HasColumnsHeights { get; set; }
        private bool HasRowHeightPx => _rowHeightPx.HasValue && _rowHeightPx > 0;
        private double RowHeightPx => HasRowHeightPx ? _rowHeightPx.Value : Magics.DefaultDataGridRowHeight;
        private ViewPortState FormerState { get; set; }        
        private readonly IDictionary<IDataGridColumn<RecordT>,TransformationMediator> _columnMessagePassing 
            = new Dictionary<IDataGridColumn<RecordT>, TransformationMediator>();
        private readonly List<Action<IWhereCollector<RecordT>>> _columnFilter = new List<Action<IWhereCollector<RecordT>>>();        
        private readonly SelectionManager<RecordT> _selection;
        private RecordT _mostRecentActivatedItem;
        private double? _oldTbodyHeight;
        private readonly VisibleRows<RecordT> _visRows = new VisibleRows<RecordT>();
        private List<GroupDescr> _groups;        
        private const int RowsPerGroupDescr = 1;

        public DataGridModelPresenter(DataGridModel<RecordT> model, ITableView view) {
            _model = model;
            _view = view;
            _selection = new SelectionManager<RecordT>(
                _model.Selected,
                _model.Items);

            _model.Activated.Changed += (_, oldValue, newValue, ___, ____) => {
                var idx = oldValue == null ? -1 : _visRows.GetRowIndexOf(oldValue);
                if (idx >= 0) {
                    _view.RowCssClassRemove(idx, Magics.CssClassRowActivated);
                }
                
                idx = newValue == null ? -1 : _visRows.GetRowIndexOf(newValue);
                if (idx >= 0) {
                    _view.RowCssClassAdd(idx, Magics.CssClassRowActivated);
                }
            };

            _model.Selected.Changed += (at, inserted, removed) => {
                removed.ForEach(x => {
                    var idx = _visRows.GetRowIndexOf(x);

                    if (idx >= 0) {
                        _view.RowCssClassRemove(idx, Magics.CssClassRowSelected);
                    }
                });

                inserted.ForEach(x => {
                    var idx = _visRows.GetRowIndexOf(x);

                    if (idx >= 0) {
                        _view.RowCssClassAdd(idx, Magics.CssClassRowSelected);
                    }
                });
            };
        }

        private List<GroupDescr> BuildGroups() {
            return _model.CurrentGroupingLogic.GroupRecords(_model.Items, _model.Columns).ToList();
        }
        
        public void RebuildGroups(RebuildGroupsReason reason) {
            _groups = BuildGroups();
            Logger.Debug(GetType(), "Groups start due to {0}", reason);
            _groups.ForEach(x => Logger.Debug(GetType(), "Groups item {0}", x));
            Logger.Debug(GetType(), "Groups end");
        }

        private GroupDescr GetGroupForTrailerOrNull(int visualRecordIdx) {
            return _groups.FirstOrDefault(x => x.TillVisIdx == visualRecordIdx);
        }

        private string GetRowCssClassOrNull(RecordT inp) {
            return _model.CustomRowCssClassOrNull?.Invoke(inp);
        }

        private void SetRowCssClassIfNeeded(RecordT inp, int pos) {
            var cl = GetRowCssClassOrNull(inp);
            if (cl == null) {
                return;
            }

            _view.RowCssClassAdd(pos, cl);
        }

        private double ComputeMaxVisibleRows() {
            return Math.Ceiling(_view.BodyVisibleSpace / RowHeightPx);
        }
        
        private double ComputeIncompleteLeadRowPad() {
            return Math.Ceiling(_view.ScrollPosition % RowHeightPx);
        }

        private double ComputeIncompleteBodyRowPad() {
            return Math.Ceiling(_view.BodyVisibleSpace % RowHeightPx);
        }

        private double ComputeFirstVisibleRow() {
            return Math.Floor(_view.ScrollPosition / RowHeightPx);
        }

        private IEnumerable<Element> BuildRow(RecordT item) {
            return _model.Columns.Select(x => x.CreateAndInitViewFor(item));
        }

        private void DestroyRowAt(int position) {
            switch (_visRows.GetRowTypeAt(position)) {
                case DataOrGroup.Group: 
                    Logger.Debug(GetType(), "Removing group at {0}", position);
                    break;
                
                case DataOrGroup.Data: 
                    var record = _visRows.GetDataAt(position);
                    Logger.Debug(GetType(), "Removing data {0} at {1}", record, position);
                    _model.Columns.ForEach(x => x.DeleteViewFor(record));
                    break;
                
                default: throw new Exception("unsupported DataOrGroup");
            }
            _view.DeleteBodyRow(position);
            _visRows.RemoveAt(position);
        }
        
        private void AppendGroupTrailer(GroupDescr grp) {
            var elems = _model.CurrentGroupingLogic.BuildSummaryRow(_model.Items, grp, _model.Columns);
            _visRows.Append(grp);

            var pos = _view.BodyRowCount;
            _view.InsertBodyRow(pos, elems);
        }

        private void AppendRowFor(RecordT item) {
            _visRows.Append(item); 
            var elems = BuildRow(item);
            var pos = _view.BodyRowCount;
            _view.InsertBodyRow(pos, elems);

            if (_model.Selected.Contains(item)) {
                _view.RowCssClassAdd(pos, Magics.CssClassRowSelected);    
            } else {
                SetRowCssClassIfNeeded(item, pos);
            }
        }
        
        private void InsertGroupTrailer(int pos, GroupDescr grp) {
            _visRows.Insert(pos, grp);
            var elems = _model.CurrentGroupingLogic.BuildSummaryRow(_model.Items, grp, _model.Columns);
            _view.InsertBodyRow(pos, elems);
        }

        private void InsertRowFor(int pos, RecordT item) {
            _visRows.Insert(pos, item);
            var elems = BuildRow(item);
            _view.InsertBodyRow(pos, elems);

            if (_model.Selected.Contains(item)) {
                _view.RowCssClassAdd(pos, Magics.CssClassRowSelected);    
            } else {
                SetRowCssClassIfNeeded(item, pos);
            }
        }

        private void DeleteAllBodyRows() {
            for (var i=_view.BodyRowCount; i>0; i--) {
                DestroyRowAt(0);
            }
        }

        private void DeleteAllHeaderRows() {
            for (var i=_view.HeaderRowCount; i>0; i--) {
                _view.DeleteHeaderRow(0);
            }
        }
        
        private int GetExtraGroupRowsCount(int beforeVisualIdx) {
            return _groups.Count(x => x.TillVisIdx < beforeVisualIdx);
        }

        private void InitializeColumnWidths() {
            var heightPxOrNull = _view.GetBodyRowHeight(0);
            
            if (heightPxOrNull.HasValue) {
                Logger.Debug(GetType(),"Datagrid returned row height {0}", heightPxOrNull);
                _rowHeightPx = heightPxOrNull.Value;
            }
            
            _model.Columns.ForEachI((i,c) => {
                var widthPx = _view.GetFactColumnWidth(i);
                if (widthPx.HasValue) {
                    Logger.Debug(GetType(),"column {0} at {1} has computed width {2}", c, i, widthPx);
                    c.ComputedWidth = widthPx.Value;
                }
            });
            
            _model.Columns.ForEachI((colIdx,col) => _view.SetColumnWidth(colIdx, col.Width));
            HasColumnsHeights = true;
        }

        private void OnItemsAdded(int _, RecordT[] __) {
            //for simplicity just recreate screen (because f.e. new rows may partially replace visible part)
            //additionally: row number column (if any) means that potentially all visible rows need to be updated

            Logger.Debug(GetType(),"OnItemsAdded()");
            PopulateRows(true);
        }

        private void OnItemsDeleted(RecordT[] _) {
            //for simplicity just recreate screen (because it can be that either visible rows are removed and potentially following rows should be added). 
            //additionally: row number column (if any) means that potentially all visible rows need to be updated

            Logger.Debug(GetType(),"OnItemsDeleted()");
            PopulateRows(true);
        }

        private void OnScrolled() {
            //compare old and new State. 
            //See if any row should stay on screen 
            //  if not then just redraw
            //  if yes AND scroll-to-top then prepend new rows and delete trailing rows 
            //  if yes AND scroll-to-bottom then append new rows and delete leading rows
            Logger.Debug(GetType(),"OnScrolled() starting");
            PopulateRows(false);
            Logger.Debug(GetType(),"OnScrolled() finished");
        }

        private void OnAttached() {
            Logger.Debug(GetType(),"OnAttached() starting");
            PopulateRows(false, !FormerState.IsEmpty); //maybe just recreate screen
            Logger.Debug(GetType(),"OnAttached() finished");
        }
        
        private void OnResized() {
            //see how many trailing rows should be added or removed
            Logger.Debug(GetType(),"OnResized() starting");
            PopulateRows(false);
            Logger.Debug(GetType(),"OnResized() finished");
        }

        private void OnInitialized() {
            Logger.Debug(GetType(),"OnInitialized() starting");
            RebuildGroups(RebuildGroupsReason.Initialization);
            PopulateRows(true); //just recreate screen
            Logger.Debug(GetType(),"OnInitialized() finished");
        }
        
        private ViewPortState CalculateState() {
            Logger.Debug(GetType(),
                "CalculateState()'s input HasRowHeightPx?={0} tbodyFactHeightPx={1} theadFactHeight={2} allItemsCount={3} rowHeightPx={4} topScrollPosPx={5}",
                HasRowHeightPx,
                _view.BodyVisibleSpace, 
                _view.HeaderVisibleSpace,
                _model.Items.Length + _groups.Count * RowsPerGroupDescr,
                RowHeightPx,
                _view.ScrollPosition
            );
            
            var result = new ViewPortState {
                RawScrollPos = _view.ScrollPosition,
                PhysicalRowsCount = _model.Items.Length,
                VirtualRowsCount = _groups.Count * RowsPerGroupDescr,
                AllRowsCount = _model.Items.Length + _groups.Count * RowsPerGroupDescr,
                TheoreticalVisibleRowsCount = (int)ComputeMaxVisibleRows(),
                FirstVisibleRow = (int)ComputeFirstVisibleRow(),
                IncompleteLeadRowPad = ComputeIncompleteLeadRowPad(),
                IncompleteBodyPad = ComputeIncompleteBodyRowPad()
            };

            Logger.Debug(GetType(),"CalculateState()'s raw outcome={0}", result);

            if (result.FirstVisibleRow > result.AllRowsCount || result.FactVisibleRowsCount < 0) {
                //scroll is past data (probably many rows were deleted / grouping changed)
                result.IsInvalidScrollPos = true;

                if (result.AllRowsCount >= result.TheoreticalVisibleRowsCount) {
                    //will scroll to real bottom of new view
                    result.FirstVisibleRow = result.AllRowsCount - result.TheoreticalVisibleRowsCount;
                    result.RawScrollPos = result.FirstVisibleRow * RowHeightPx;
                } else {
                    //will just move to beginning of table
                    result.FirstVisibleRow = 0;
                    result.RawScrollPos = 0;
                }
                
                Logger.Debug(GetType(),"CalculateState()'s adjusted outcome={0}", result);
            }
            
            return result;
        }
        
        private void PopulateRows(bool itemsChanged, bool shouldRestoreScrollPosition = false) {
            var oldState = FormerState ?? ViewPortState.CreateEmpty();
            Logger.Debug(GetType(),"Former state: {0} shouldRestoreScrollPosition={1}", oldState, shouldRestoreScrollPosition);

            if (InitializeRowHeightIfNeededAndPossible()) {
                UpdateTbodyHeight();
            }

            if (shouldRestoreScrollPosition && oldState.IsEmpty) {
                shouldRestoreScrollPosition = false;
            }

            var newState = shouldRestoreScrollPosition ? oldState : CalculateState();

            if (shouldRestoreScrollPosition || newState.IsInvalidScrollPos) {
                _view.ScrollPosition = oldState.RawScrollPos;
            }
            
            Logger.Debug(GetType(),"New state: {0}", newState);
            Logger.Debug(GetType(),"Scrolled progressed rows: {0} rowHeight={1}", newState.FirstVisibleRow - oldState.FirstVisibleRow, RowHeightPx);
            
            //see if any rows can be preserved by checking whether windows overlap
            var overlap = new Overlap(
                itemsChanged,
                oldState, 
                newState);
            Logger.Debug(GetType(),"calculated overlap: {0}", overlap);

            

            //delete not overlapped leading rows (present in old BUT not present in new)
            if (overlap.ToPreTrim.IsNonEmpty) {
                Logger.Debug(GetType(),"overlap - removing unneccessary {0} leading rows", overlap.ToPreTrim.Length);
                for (var i = overlap.ToPreTrim.Length; i > 0; i--) {
                    DestroyRowAt(0);
                }
            }
            


            //delete not overlapped trailing rows
            if (overlap.ToPostTrim.IsNonEmpty) {
                Logger.Debug(GetType(),"overlap - removing unneccessary {0} trailing rows", overlap.ToPostTrim.Length);

                for (var i = overlap.ToPostTrim.Length; i > 0; i--) {
                    var pos = _view.BodyRowCount-1;
                    DestroyRowAt(pos);
                }
            }
            


            //prepend not overlapped rows 
            if (overlap.ToPrepend.IsNonEmpty) {
                Logger.Debug(GetType(),"overlap - prepending new {0} leading rows", overlap.ToPrepend.Length);
                var idx = 0;
                
                foreach (var item in EnumerateRecords(overlap.ToPrepend).AsEnumerable()) {
                    switch (item.Item1) {
                        case DataOrGroup.Data:
                            Logger.Debug(GetType(), "Prepending item {0}", item.Item2);
                            InsertRowFor(idx++, item.Item2);
                            break;

                        case DataOrGroup.Group:
                            Logger.Debug(GetType(), "Prepending group trailer for grp {0}", item.Item1);
                            InsertGroupTrailer(idx++, item.Item3);
                            break;

                        default: throw new Exception("unsupported DataOrGroup");
                    }
                }
            }
            
            

            //append not overlapped rows 
            if (overlap.ToAppend.IsNonEmpty) {
                Logger.Debug(GetType(),"overlap - appending new {0} trailing rows", overlap.ToAppend.Length);
                
                foreach (var item in EnumerateRecords(overlap.ToAppend).AsEnumerable()) {
                    switch (item.Item1) {
                        case DataOrGroup.Data:
                            Logger.Debug(GetType(), "Appending item {0}", item.Item2);
                            AppendRowFor(item.Item2);
                            break;

                        case DataOrGroup.Group:
                            Logger.Debug(GetType(), "Appending group trailer for grp {0}", item.Item3);
                            AppendGroupTrailer(item.Item3);
                            break;
                            
                        default: throw new Exception("unsupported DataOrGroup");
                    }
                }
            }
            
            var padBodyToBottom = 
                newState.FirstVisibleRow == 0 || 
                newState.LastVisibleRow != (_model.Items.Length + _groups.Count*RowsPerGroupDescr - 1);
            
            Logger.Debug(GetType(), "Former space lead={0} trail={1}", _view.LeadSpace, _view.TrailSpace);
            _view.FirstRowIndex = newState.FirstVisibleRow;
            _view.LeadSpace = newState.FirstVisibleRow*RowHeightPx
                              + (padBodyToBottom ? 0 : newState.IncompleteBodyPad + newState.IncompleteLeadRowPad);
            _view.TrailSpace = 
                (newState.AllRowsCount - newState.FirstVisibleRow - newState.FactVisibleRowsCount)*RowHeightPx
                + (padBodyToBottom ? newState.IncompleteBodyPad + newState.IncompleteLeadRowPad : 0);

            Logger.Debug(GetType(), "New space lead={0} trail={1} FirstVisibleRow={2}", _view.LeadSpace, _view.TrailSpace, newState.FirstVisibleRow);
			
            FormerState = newState;

            //_visRows.DebugPrintContents();
            //((HtmlTableBasedTableView)_view).DebugPrintContents(1);
        }

        private IEnumerator<Tuple<DataOrGroup,RecordT,GroupDescr>> EnumerateRecords(Range rng) {
            var rngFromSafe = rng.From < 0 ? 0 : rng.From; //may be negative
            var groupRecords = GetExtraGroupRowsCount(rngFromSafe);
            var needed = rng.Length;
            var iPhy = rngFromSafe - groupRecords;
            var iVis = rngFromSafe;
            var actuallyReturned = 0;
            Logger.Debug(GetType(), "EnumerateRecords({0}) iVis={1} iPhy={2} needed={3}", rng, iVis, iPhy, needed);
            
            var trailGrp = GetGroupForTrailerOrNull(iVis);
            if (trailGrp != null) {
                needed--;
                actuallyReturned++;
                yield return new Tuple<DataOrGroup,RecordT,GroupDescr>(DataOrGroup.Group, default(RecordT), trailGrp);
                iVis++;
            }
            
            while (needed > 0) {
                actuallyReturned++;
                yield return new Tuple<DataOrGroup,RecordT,GroupDescr>(DataOrGroup.Data, _model.Items[iPhy], null);

                needed--;
                iVis++;
                iPhy++;

                if (needed <= 0) {
                    break;
                }
                
                var grpOrNull = GetGroupForTrailerOrNull(iVis);
                if (grpOrNull != null) {
                    needed--;
                    actuallyReturned++;
                    yield return new Tuple<DataOrGroup,RecordT,GroupDescr>(DataOrGroup.Group, default(RecordT), grpOrNull);
                    iVis++;
                }
            }

            Logger.Debug(GetType(), "EnumerateRecords({0}) iVis={1} iPhy={2} needed={3} actuallyReturned={4}", rng, iVis, iPhy, needed, actuallyReturned);
        }

        /// <summary>
        /// returns: if succeeded (if was able to initialize it)
        /// </summary>
        private bool InitializeRowHeightIfNeededAndPossible() {
            if (HasRowHeightPx && HasColumnsHeights || !_model.Items.Any() || !_view.IsAttached() || !_view.GetHeaderRowHeight().HasValue) {
                return false;
            }

            Logger.Debug(GetType(),"Needs and is able to calculate datagrid's row height and fact column widths");
            var needsProto = _visRows.GetVisibleCount() <= 0;
            
            if (needsProto) {
                AppendRowFor(_model.Items.First());
            }

            InitializeColumnWidths();

            if (needsProto) {
                DestroyRowAt(0);
            }

            return true;
        }
        
        private async Task ExportDataGrid() {
            Logger.Debug(GetType(), "Export trigger start");
            await DataGridSettings.ConvertJsonToXlsxOperation(_model.ExportDataToJson());
            await Task.Delay(1000); //eyecandy: so that spinner is visible
            Logger.Debug(GetType(), "Export trigger finished");
        }

        public void Initialize() {
            _columnFilter.Clear();
            DeleteAllHeaderRows();
            DeleteAllBodyRows();

            var header = _model.Columns.SelectI((i,x) => {
                var messagePassing = new TransformationMediator(
                        y => OnUserColumnFilterChanged(i, y),
                        y => {
                            Logger.Debug(GetType(), 
                                "User grouping changing for column {0} - now disabling other possibly grouped columns", x.Name);
                            _model.Columns
                                .Where(z => z!=x && z.IsGrouped)
                                .ForEach(a => {
                                    Logger.Debug(GetType(), 
                                        "Disabling grouping for column {0}", a.Name);
                                    _columnMessagePassing[a].ProgrammaticGroupingChangedHandler(null);
                                });
                            OnUserColumnGroupingChanged(i, y); },
                        y => OnUserColumnAggregationChanged(i, y)
                    );
                _columnMessagePassing.Add(x, messagePassing);
                
                var elem = x.CreateColumnHeaderController(messagePassing);
                
                _columnFilter.Add(x.CurrentFilterImpl);
                
                var result = new HeaderColumnDescr(
                    x.CreateHeaderLabel(), 
                    elem ?? new HTMLSpanElement(), 
                    x.Orderable,
                    x.IsSelectionHandler);

                x.OnModelAttached(_model);
                return result;
            }).ToList();

            _view.InsertHeader(header);
            
            _model.Items.Changed += (insertAt, inserted, deleted) => {
                RebuildGroups(RebuildGroupsReason.ItemsInModelChanged);
                
                if (deleted.Any()) {
                    OnItemsDeleted(deleted);
                }
                
                if (inserted.Any()) {
                    OnItemsAdded(insertAt, inserted);
                }

                if (_selection.Any() && !_disableSelectionChangedPropagation) {
                    //slows down sorting
                    var reallyDeleted = deleted.Where(x => !inserted.Contains(x)).ToArray();
                    _selection.OnItemsRemovedFromModel(reallyDeleted);
                }
            };

            _view.Scrolled += OnScrolled;
            _view.RerenderNeeded += OnViewChanged;
            
            _view.FilterRows += (type, newValue) => _model.ChangeGlobalFilterToContainsCaseInsensitive(type, newValue ?? "");
            _view.Export += holder => holder.Value = ExportDataGrid();

            _view.RowSelected += (tbodyRowIdx, mode) => {
                Logger.Debug(GetType(), "selection maybe changing to {0} using mode {1}", tbodyRowIdx, mode);
                
                var itm = ConvertVisualIdxToPhysicalItem(tbodyRowIdx);

                if (!itm.HasValue) {
                    Logger.Debug(GetType(), "selection not changing as non-physical-record was selected");
                    return;
                }

                switch (mode) {
                    case RowSelectionMode.ReplaceWithOne: 
                        _selection.ReplaceWithItem(itm.Value);
                        break;

                    case RowSelectionMode.AddOrRemoveOne:
                        _selection.Toggle(itm.Value);
                        break;
                        
                    case RowSelectionMode.AddRange:
                        if (_mostRecentActivatedItem == null || !_model.Items.Contains(_mostRecentActivatedItem)) {
                            //reference row is invalid
                            _selection.ReplaceWithItem(itm.Value);
                        } else {
                            _selection.ReplaceWithRange(_mostRecentActivatedItem, itm.Value);
                        }
                        break;

                    default: throw new ArgumentException("unsupported RowSelectionMode");
                }
                _mostRecentActivatedItem = itm.Value;
            };

            _view.RowActivated += async tbodyRowIdx => {
                Logger.Debug(GetType(), "rowaactivated maybe {0}", tbodyRowIdx);
                var record = ConvertVisualIdxToPhysicalItem(tbodyRowIdx);
                
                if (!record.HasValue) {
                    Logger.Debug(GetType(), "rowaactivated not invoking as non-physical-record was choosen");
                    return;
                }

                await _model.Activated.DoChange(record.Value, true, this, false);
            };

            _view.Reloadable = _model.ReloadDataActionOrNull != null;
            _view.ReloadData += () => _model.ReloadDataActionOrNull?.Invoke();
            _view.ColumnOrderChanged += (columnNo, order) => OnSortingChanged(columnNo, order);
            
            OnInitialized();
        }

        private double? UpdateTbodyHeight() {
            var newTbodyHeight = _model.TbodyHeightProvider(_view.Widget.ParentElement, _view.HeaderVisibleSpace, _rowHeightPx);

            if (newTbodyHeight.HasValue && newTbodyHeight.Value > 0) {
                _view.BodyVisibleSpace = newTbodyHeight.Value;
                Logger.Debug(GetType(),"grid tbody updated to new height {0} using rowHeight {1} and headerHeight {2}", newTbodyHeight.Value, _rowHeightPx, _view.HeaderVisibleSpace);
            } else {
                Logger.Debug(GetType(),"grid tbody not updated because of missing height");
            }
            return newTbodyHeight;
        }

        private void OnViewChanged(RerenderReason x) {
            var newTbodyHeight = UpdateTbodyHeight();
            Logger.Debug(GetType(),"RerenderNeeded state _oldTbodyHeight={0} newTbodyHeight={1}", _oldTbodyHeight, newTbodyHeight);

            switch (x) {
                case RerenderReason.AttachedToDom:
                    OnAttached();

                    var postAttachHeight = UpdateTbodyHeight();

                    if (postAttachHeight.HasValue && newTbodyHeight.HasValue && 
                        !postAttachHeight.Value.AreApproximatellyTheSame(newTbodyHeight.Value)) {

                        Logger.Debug(GetType(),"grid height updated after OnAttached from {0} to {1} so doing resize", newTbodyHeight, postAttachHeight);
                        OnResized();                                
                    } else {
                        Logger.Debug(GetType(),"grid height not not updated after OnAttached");
                    }
                        
                    break;

                case RerenderReason.FilterStateChanged:
                case RerenderReason.ElementResized:
                    if (newTbodyHeight.HasValue &&
                        (!_oldTbodyHeight.HasValue || !newTbodyHeight.Value.AreApproximatellyTheSame(_oldTbodyHeight.Value))) {

                        Logger.Debug(GetType(),"grid height did change so propagating resize"); 
                        OnResized();
                    } else {
                        Logger.Debug(GetType(),"grid height didn't really change so not doing resize");
                    }
                        
                    break;

                default: throw new Exception("unsupported RerenderReason");
            }

            _oldTbodyHeight = newTbodyHeight;
        }

        private int RelativeVisualIdxToAbsoluteVisualIdx(int tbodyRowIdx) {
            return tbodyRowIdx + FormerState.FirstVisibleRow;
        }
        
        private Optional<RecordT> ConvertVisualIdxToPhysicalItem(int tbodyRowIdx) {
            var visualIdx = RelativeVisualIdxToAbsoluteVisualIdx(tbodyRowIdx);
            if (GetGroupForTrailerOrNull(visualIdx) != null) {
                return Optional<RecordT>.CreateNone();
            }

            var groupRecordsCnt = GetExtraGroupRowsCount(visualIdx);
            return Optional<RecordT>.CreateSome(_model.Items[visualIdx - groupRecordsCnt]);
        }

        private void OnSortingChanged(int columnNo, SortOrder order) {
            //when user changes sorting for a given column he expects that other columns loose sorting
            _model.Columns.ForEachI((i,c) => {
                if (columnNo != i && c.Orderable) {
                    _view.SetColumnOrder(i, SortOrder.Unspecified);    
                }
            });

            try {
                _disableSelectionChangedPropagation = true;
                
                _model.ChangeItemsSorting(collector => {
                    var column = _model.Columns[columnNo];
                    column.SortingRuleApply(collector, order);
                });
            } finally {
                _disableSelectionChangedPropagation = false;
            }
        }

        public void ScrollToItem(RecordT itm) {
            Logger.Debug(GetType(), "scrolling to item {0}", itm);
            var visIdx = _visRows.GetRowIndexOf(itm);
            var phsIdx = _model.Items.IndexOf(itm);

            Logger.Debug(GetType(), "scrolling vidx={0} pidx={1}", visIdx, phsIdx);

            if (visIdx >=0 && visIdx < _visRows.GetVisibleCount() - 2) {
                return;
            }

            if (phsIdx < 0) {
                return;
            }
            
            _view.ScrollPosition = RowHeightPx * phsIdx;
        }

        private void OnUserColumnGroupingChanged(int columnIdx, ChangeOrRemove changeOrRemove) {
            Logger.Debug(GetType(), "OnColumnGroupingChanged()");
            
            RebuildGroups(RebuildGroupsReason.UserCausedGroupingChange);
            PopulateRows(true);
        }

        private void OnUserColumnAggregationChanged(int columnIdx, ChangeOrRemove changeOrRemove) {
            Logger.Debug(GetType(), "OnColumnAggregationChanged()");

            PopulateRows(true);
        }
        
        private void OnUserColumnFilterChanged(int columnIdx, ChangeOrRemove type) {
            Logger.Debug(GetType(), "OnColumnFilterChanged({0},{1})", columnIdx, type);

            _model.ChangeColumnFilter(collector => {
                var colFilter = _columnFilter[columnIdx];

                switch (type) {
                    case ChangeOrRemove.Removed:
                        break;

                    case ChangeOrRemove.Changed:
                        colFilter(collector);
                        break;
                
                    default:
                        Logger.Error(GetType(), "Unknown FilterChangeType");
                        throw new Exception("Unknown FilterChangeType");
                }
            });
        }

        public void InitGrouping(IDataGridColumn<RecordT> column, string groupFuncLabel) {
            _columnMessagePassing[column].ProgrammaticGroupingChangedHandler(groupFuncLabel);
            RebuildGroups(RebuildGroupsReason.ProgrammaticGroupingChange);
        }

        public void InitAggregation(params Tuple<IDataGridColumn<RecordT>,string>[] columnToAggregationLabel) {
            columnToAggregationLabel.ForEach(
                x => _columnMessagePassing[x.Item1].ProgrammaticAggregationChangedHandler(x.Item2));

            RebuildGroups(RebuildGroupsReason.ProgrammaticAggregationChange);
        }
    }
}
