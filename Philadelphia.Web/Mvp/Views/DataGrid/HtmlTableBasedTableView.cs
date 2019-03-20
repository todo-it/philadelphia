using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class HtmlTableBasedTableView : ITableView {
        /*
         * implementation notes: 
         * watch out that it assumes that it has one leading and one trailing body row. 
         * Because of this you see many expressions containing magic '1' number 
         */

        private double?[] _columnWidths = new double?[0];
        private readonly HTMLElement _tblContainer,_tblSettings;
        private readonly HTMLTableElement _tbl;
        private readonly HTMLElement _tbody, _thead, _leadRow, _trailRow;
        private readonly List<Element> _sortControls = new List<Element>();
        
        private readonly InputView _searchBox;
        private readonly InputTypeButtonActionView _exportActionView,_reloadDataAction;
        private bool _filterable = true;
        private int? _isResizingColumn = null;
        private Optional<double> _lastScrollPos = Optional<double>.CreateNone();

        public event Action<RerenderReason> RerenderNeeded;
        public event Action Scrolled;
        public event Action<int,SortOrder> ColumnOrderChanged;
        public event Action<int,RowSelectionMode> RowSelected;
        public event Action<int> RowActivated;
        public event Action<FilterRowsAction,string> FilterRows;
        public event Action<MutableHolder<Task>> Export;
        public event Action ReloadData;

        public SingleRowSelectionMode SingleClickMeaning {get; set; } = SingleRowSelectionMode.ReplaceSelection;

        public int FirstRowIndex {
            set => _tbody.AddOrRemoveClass(value % 2 > 0, Magics.CssClassFirstRowIsOdd);
        }
        public double LeadSpace {
            get => _leadRow.ClientHeight;
            set => _leadRow.Style.Height = value+"px";
        }
        public double TrailSpace {
            get => _trailRow.ClientHeight;
            set => _trailRow.Style.Height = value+"px";
        }
        public double ScrollPosition {
            get => _tbody.GetScrollTop();
            set => _tbody.SetScrollTop(value);
        }

        public double HeaderVisibleSpace => _thead.GetBoundingClientRect().Height;

        public double BodyVisibleSpace {
            get => _tbody.GetBoundingClientRect().Height;
            set => _tbody.Style.Height = value+"px";
        }

        public HTMLElement Widget => _tblContainer;
        public int HeaderRowCount { get; private set;}
        public int BodyRowCount { get; private set;}

        public IEnumerable<IView<HTMLElement>> Actions => 
            new List<IView<HTMLElement>> {_reloadDataAction,_exportActionView,_searchBox};
        public bool Sortable { get; set; } = true; 
        public bool Groupable { set => _tblSettings.Style.SetProperty("display", value ? "" : "none"); }
        
        public bool Reloadable {
            get => !_reloadDataAction.Widget.ClassList.Contains(Magics.CssClassNotRendered);
            set => _reloadDataAction.Widget.AddOrRemoveClass(!value, Magics.CssClassNotRendered);
        }

        public bool Resizable { get; set; } = true;
        
        public bool Filterable {
            get => _filterable;
            set {
                _filterable = value;
                _tbl.AddOrRemoveClass(_filterable, Magics.CssClassFilterable);
                Logger.Debug(GetType(), "Table filterable={0}", value);
            }
        }

        public HtmlTableBasedTableView() {
            _reloadDataAction = InputTypeButtonActionView.CreateFontAwesomeIconedButtonLabelless(
                Magics.FontAwesomeReloadData);
            _reloadDataAction.Widget.ClassList.Add(Magics.CssClassNotRendered);
            _reloadDataAction.Widget.SetAttribute(Magics.AttrAlignToRight, "");
            
            _reloadDataAction.Triggered += () => {
                Logger.Debug(GetType(), "Reload data triggered");
                ReloadData?.Invoke();
            };
	        
            _exportActionView = new InputTypeButtonActionView("");
            _exportActionView.Widget.ClassList.Add(Magics.CssClassDatagridAction);
            _exportActionView.Widget.SetAttribute(Magics.AttrAlignToRight, "");

            var img = new HTMLImageElement {
                Src = Magics.IconUrlExportToXlsx
            };
            _exportActionView.Widget.AppendChild(img);
	        
            LocalActionBuilder.Build(
                _exportActionView, 
                async () => {
                    if (Export == null) {
                        return;
                    }

                    //yes, it is controversial but:
                    // I want extraction to be triggered by "dummy" view and consumed by presenter. 
                    // Presenter knows real "rich" columns in datagrid. 
                    // I want button to have spinner for the time that conversion happens
                    var result = new MutableHolder<Task>();
                    Export(result);
                    await result.Value;
                });
            
            _searchBox = new InputView("") {
                Clearable = true,
                PlaceHolder = I18n.Translate("Search rows...")
            };
            _searchBox.Widget.SetAttribute(Magics.AttrAlignToRight, "");
            _searchBox.Widget.ClassList.Add(Magics.CssClassSearchBox);
            _searchBox.Changed += (newValue, _) => {
                Logger.Debug(GetType(), "Search term changed to {0}", newValue);
                FilterRows?.Invoke("".Equals(newValue) ? FilterRowsAction.Remove : FilterRowsAction.Change, newValue);
            };
            
            _tblContainer = new HTMLDivElement {
                Id = UniqueIdGenerator.GenerateAsString(),
                ClassName = GetType().FullName };
            
            _tbl = new HTMLTableElement {
                Id = UniqueIdGenerator.GenerateAsString(),
                ClassName = GetType().FullName
            };
            Filterable = Filterable; //init css class
            Resizable = Resizable; //init css class

            _tblContainer.OnMouseOver += ev => _tblContainer.AddClasses(Magics.CssClassActive);
            _tblContainer.OnMouseOut += ev => _tblContainer.RemoveClasses(Magics.CssClassActive);

            _tblContainer.AppendChild(_tbl);

            _tblSettings = new HTMLAnchorElement {Href = "#", TextContent = Magics.FontAwesomeBars};
            _tblSettings.AddClasses(Magics.CssClassSettingsAction);
            
            _tblContainer.AppendChild(_tblSettings);

            _thead = _tbl.CreateTHead();
            _thead.RemoveClasses(Magics.CssClassActive);
            _thead.AddClasses(Magics.CssClassInactive);

            _tbody = new HTMLElement("tbody");
            _tbl.AppendChild(_tbody);

            _leadRow = new HTMLTableRowElement {Id = UniqueIdGenerator.GenerateAsString()};
            _tbody.AppendChild(_leadRow);

            _trailRow = new HTMLTableRowElement {Id = UniqueIdGenerator.GenerateAsString()};
            _tbody.AppendChild(_trailRow);
            _tbody.OnClick += x => {
                if (!x.HasHtmlTarget()) {
                    return;
                }
                var clickedElem = x.HtmlTarget();
                
                if (_thead.Children.Length >= 1) {
                    //when table has header row...
                    var td = clickedElem.TagName == "TD" ? clickedElem : clickedElem.GetParentElementOfTypeOrNull("TD");

                    if (td != null ) {
                        //...and user clicked within cell
                        var colNo = td.ParentElement.IndexOfChild(td);
                        var th = _thead.Children[0].Children[colNo];

                        //... that says that it handles selection events...
                        if (th.HasAttribute(Magics.AttrDataSelectionHandler)) {
                            Logger.Debug(GetType(), "row selection - special, handled by cell");
                            return; //...then let it handle it
                        }
                    }
                }

                var row = clickedElem.GetParentElementOfTypeOrNull("TR", _tbody);
                if (row == null) {
                    return;
                }
                var tr = (HTMLTableRowElement)row;
                var trIndex = tr.ParentElement.IndexOfChild(tr); //cannot use tr.RowIndex that works fine in Chrome&FF because it always returns zero in IE11 
                Logger.Debug(GetType(), "row selection - regular, handled by grid at index {0}", trIndex);
                
                var selectionMode = ComputeSelectionModeFrom(x);
                
                //prevent text selection on shift click
                if (selectionMode == RowSelectionMode.AddRange) {
                    //I would bet that canceling selection is done by event.PreventDefault() but apparently it isn't
                    Document.GetSelection().RemoveAllRanges();
                }
                
                RowSelected?.Invoke(
                    trIndex - 1, //leading virtual row
                    selectionMode);
            };
            _tbody.OnDblClick += x => {
                if (!x.HasHtmlTarget()) {
                    return;
                }
                var clickedElem = x.HtmlTarget();
                var row = clickedElem.GetParentElementOfTypeOrNull("TR", _tbody);
                if (row == null) {
                    return;
                }
                var tr = (HTMLTableRowElement)row;
                
                RowActivated?.Invoke(
                    tr.ParentElement.IndexOfChild(tr) //cannot use tr.RowIndex that works fine in Chrome&FF because it always returns zero in IE11 
                    - 1 //leading virtual row
                );
            };
            _tbody.AddEventListener("scroll", () => {
                var h = _tbody.GetScrollTop();
                var factChildrenHeight = _tbody.Children.Sum(x => ElementExtensions.GetClientDimensionsOrNull(x)?.Height ?? 0.0 );

                var ignoring = _lastScrollPos.HasValue && Math.Abs(_lastScrollPos.Value - h) < Magics.ScrollingPxToIgnore;
                Logger.Debug(GetType(), "before onscroll handler scrollTop={0} childrenSumHeight={1} bodyHeight={2} lastScrollPos={3}", 
                    h, factChildrenHeight, _tbody.ClientHeight, _lastScrollPos, ignoring);
                
                if (ignoring) {
                    return;
                }

                Scrolled?.Invoke();
                
                factChildrenHeight = _tbody.Children.Sum(x => x.GetClientDimensionsOrNull()?.Height ?? 0.0 );
                Logger.Debug(GetType(), "after onscroll handler scrollTop={0} childrenSumHeight={1} bodyHeight={2}", h, factChildrenHeight, _tbody.ClientHeight);
                _lastScrollPos = Optional<double>.CreateSome(h);
                _tbody.SetScrollTop(h); //without it infinite back and forth scroll happens for some reason
            });
            _tblSettings.OnClick += ev => {
                Logger.Debug(GetType(), "activating filter");
                ev.PreventDefault();

                if (!Sortable && !Filterable) {
                    return;
                }
                
                _thead.RemoveClasses(Magics.CssClassInactive);
                _thead.AddClasses(Magics.CssClassActive);
                RerenderNeeded?.Invoke(RerenderReason.FilterStateChanged); //filter activated
            };
            
            DocumentUtil.AddMouseMoveListener(_thead, ev => {
                Logger.Debug(GetType(), "maybe resizing");

                if (!_isResizingColumn.HasValue) {
                    return;
                }
                var curWidths = GetColumnWidths(_isResizingColumn.Value+1); 
                var curWidth = curWidths[_isResizingColumn.Value].GetValueOrDefault();
                var totalWidth = curWidths
                    .Take(_isResizingColumn.Value+1)
                    .Sum()
                    .GetValueOrDefault();
                
                var totalOffsetLefts = _tblContainer.GetTotalOffsetLeft();
                var totalScrollLefts = _tblContainer.EnumerateSelfAndAncestors().Sum(x => x.ScrollLeft);
                var totalBorders = 3 * (_isResizingColumn.Value+1);
                var newWdth = 
                    curWidth + 
                    ev.PageX + totalScrollLefts - totalWidth - totalBorders - totalOffsetLefts;
                var diff = newWdth - curWidths[_isResizingColumn.Value].GetValueOrDefault();
                Logger.Debug(GetType(), 
                    "curWdth={0} newWdth={1} diff={2} (ev.ScreenX={3} totalScrollLefts={4} totalWidth={5} ev.PageX={6} totalOffsetLefts={7})", 
                    curWidth, 
                    newWdth,
                    diff,
                    ev.ScreenX,
                    totalScrollLefts,
                    totalWidth,
                    ev.PageX,
                    totalOffsetLefts);
                
                var isNoise = Math.Abs(diff - 2) < 5;
                if (isNoise || 
                    newWdth < Magics.MinUserResizeColumnWidth ||
                    newWdth > Magics.MaxUserResizeColumnWidth) { 
                    return;
                }

                SetColumnWidth(_isResizingColumn.Value, newWdth);
            });

            DocumentUtil.AddMouseUpListener(_thead, ev => {
                Logger.Debug(GetType(), "maybe deactivating resizer");
                _tbl.RemoveClasses(Magics.CssClassIsResizing);

                if (_isResizingColumn.HasValue) {
                    _isResizingColumn = null;
                }
            });

            DocumentUtil.AddMouseDownListener(_thead, ev => {
                if (!Sortable && !Filterable && !Resizable) {
                    return;
                }

                if (!ev.HasHtmlTarget()) {
                    return;
                }

                //find out if clicked item is a descendant of thead - if not hide filter row
                if (ev.HtmlTarget().IsElementOrItsDescendant(_thead) || 
                    ev.HtmlTarget().IsElementOrItsDescendant(_tblSettings)) {

                    Logger.Debug(GetType(), "not deactivating filter");
                    
                    if (ev.HtmlTarget().ClassList.Contains(Magics.CssClassResizeHandle)) {
                        var th = ev.HtmlTarget().ParentElement;
                        var tr = th.ParentElement;
                        var col = tr.IndexOfChild(th);
                        Logger.Debug(GetType(), "activating resizer for column {0}", col);
                        _tbl.AddClasses(Magics.CssClassIsResizing);
                        _isResizingColumn = col;
                    }

                    return;
                }

                Logger.Debug(GetType(), "deactivating filter");
                _thead.RemoveClasses(Magics.CssClassActive);
                _thead.AddClasses(Magics.CssClassInactive);
                RerenderNeeded?.Invoke(RerenderReason.FilterStateChanged); //filter deactivated
            });

            //potentially dozens of nodes managed by table itself. No need to traverse it / disable for performance reasons
            _tbody.MarkAsTraversable(false); 

            // resize events
            Widget.AddAttachedToDocumentEventListener(() => RerenderNeeded?.Invoke(RerenderReason.AttachedToDom));

            Widget.AddResizeEventListener(() => RerenderNeeded?.Invoke(RerenderReason.ElementResized));
        }
        
        private RowSelectionMode ComputeSelectionModeFrom(MouseEvent x) {
            if (!x.ShiftKey && !x.CtrlKey) {
                switch (SingleClickMeaning) {
                    case SingleRowSelectionMode.ReplaceSelection: return RowSelectionMode.ReplaceWithOne;
                    case SingleRowSelectionMode.ToggleRow: return RowSelectionMode.AddOrRemoveOne;
                    default: throw new Exception("unsupported SingleClickMeaning");
                }
            }

            if (x.ShiftKey) {
                return RowSelectionMode.AddRange;
            }

            return RowSelectionMode.AddOrRemoveOne;
        }

        private double?[] GetColumnWidths(int expectedElements) {
            AssureColumnWidthSize(expectedElements);
            return _columnWidths;
        }

        private void AssureColumnWidthSize(int size) {
            if (size > _columnWidths.Length) {
                Array.Resize(ref _columnWidths, size);
            }            
        }

        public void SetColumnWidth(int colIdx, double newSizePx) {
            AssureColumnWidthSize(colIdx + 1);
            _columnWidths[colIdx] = newSizePx;

            SetHeaderColumnWidth(colIdx, newSizePx);
            SetBodyColumnWidth(colIdx, newSizePx);
        }

        private void SetBodyColumnWidth(int colIdx, double width) {
            for (var i=0; i<BodyRowCount; i++) {
                var td = _tbody.Children[i+1].Children[colIdx];
                td.Style.MinWidth = width + "px";
                td.Style.MaxWidth = width + "px";
                td.Style.Width = width + "px";
            }
        }

        private void SetHeaderColumnWidth(int colIdx, double width) {
            for (var i=0; i<HeaderRowCount; i++) {
                var td = _thead.Children[i].Children[colIdx];
                td.Style.MinWidth = width + "px";
                td.Style.MaxWidth = width + "px";
                td.Style.Width = width + "px";
            }
        }
        
        public double? GetHeaderRowHeight() {
            var res = _thead.Children[0].GetClientDimensionsOrNull();
            return res?.Height;
        }
        
        public double? GetBodyRowHeight(int position) {
            var res = _tbody.Children[position+1].GetClientDimensionsOrNull();
            return res?.Height;
        }
        
        private double? GetHeaderColumnWidth(int colIdx) {
            var tr = _thead.Children[0];
            var td = tr.Children[colIdx];
            var dim = td.GetClientDimensionsOrNull();
            return dim?.Width;
        }

        private double? GetBodyColumnWidth(int colIdx) {
            if (BodyRowCount <= 0) {
                return null;
            }
            
            var tr = _tbody.Children[1];
            var td = tr.Children[colIdx];
            var dim = td.GetClientDimensionsOrNull();

            return dim?.Width;
        }

        public double? GetFactColumnWidth(int colIdx) {
            var header = GetHeaderColumnWidth(colIdx);
            var body = GetBodyColumnWidth(colIdx);
            Logger.Debug(GetType(),"GetFactColumnWidth() headerWidth={0} bodyWidth={1}", header, body);

            return new [] {header,body}.Max();
        }

        public void DeleteHeaderRow(int position) {
            _thead.RemoveChild(_thead.ChildNodes[position+1]);
            BodyRowCount--;
        }
        
        public void SetColumnOrder(int colIdx, SortOrder order) {
            Logger.Debug(GetType(), "SetColumnOrder({0}, {1})", colIdx, order);
            
            var ctrl = _sortControls[colIdx];

            SetSortOrder(ctrl, order);
        }

        private void OnSortClicked(Event ev) {
            if (!Sortable) {
                return;
            }

            if (!ev.HasHtmlTarget()) {
                return;
            }
            
            var htmlTarget = ev.HtmlTarget();
            Logger.Debug(GetType(), "OnSortClicked() fired for {0}", htmlTarget.TagName);
            
            if (ev.HtmlTarget().ClassList.Contains(Magics.CssClassResizeHandle)) {
                Logger.Debug(GetType(), "OnSortClicked() ignored as fired on handle");
                return;
            }

            var th = htmlTarget.TagName == "TH" ? 
                    htmlTarget
                : 
                    htmlTarget.GetParentElementOfTypeOrNull("TH");

            if (th == null) {
                Logger.Error(GetType(), "bug, could not find TH element");
                return;
            }

            var columnNo = _thead.FirstElementChild.IndexOfChild(th);
            if (columnNo < 0) {
                Logger.Error(GetType(), "bug, could not identify column number");
                return;
            }
            
            var sortCtrl = _sortControls[columnNo];

            var curOrder = GetSortOrder(sortCtrl);
            var newOrder = curOrder.CalculateNext();
            
            Logger.Debug(GetType(), "User wants to sort column number {0} from {1} to {2}", columnNo, curOrder, newOrder);
            
            if (curOrder == newOrder) {
                //performance improvement: order not changed
                return;
            }

            SetSortOrder(sortCtrl, newOrder);
            
            ColumnOrderChanged?.Invoke(columnNo, newOrder);
        }

        private SortOrder GetSortOrder(Element el) {
            return (SortOrder)Convert.ToInt32(el.GetAttribute(Magics.AttrDataSortOrder));
        }

        private void SetSortOrder(Element el, SortOrder order) {
            el.SetAttributeIfNeeded(Magics.AttrDataSortOrder, ((int)order).ToString()); //icon
            el.ParentElement?.SetAttributeIfNeeded(Magics.AttrDataSortOrder, ((int)order).ToString()); //for cursor pointer    

            el.TextContent = order.GetIconText();
        }

        public void InsertHeader(IEnumerable<HeaderColumnDescr> items) {
            _sortControls.Clear();
            
            var trLabels = new Element(ElementType.TableRow);            
            var trFilter = new Element(ElementType.TableRow);

            foreach (var item in items) {
                
                var thLabel = new Element(ElementType.TableHeaderCell);
                if (item.IsSelectionHandler) {
                    thLabel.SetAttribute(Magics.AttrDataSelectionHandler,"true");
                }

                var alwaysVisibleItemsContainer = new HTMLDivElement();
                alwaysVisibleItemsContainer.AddClasses(Magics.CssClassColumnLabel);
                //so that handle may overflow but regular content may not...

                var thFilter = new Element(ElementType.TableHeaderCell) {ClassName = Magics.CssClassFilter};
                if (item.IsSelectionHandler) {
                    thFilter.SetAttribute(Magics.AttrDataSelectionHandler,"true");
                }
                thFilter.AppendChild(item.FilterElem);
                
                var filterIndicator = new HTMLSpanElement {
                    ClassName = Magics.CssClassFilterIndicator,
                    TextContent = Magics.FontAwesomeFilter };

                var groupIndicator = new HTMLSpanElement {
                    ClassName = Magics.CssClassGroupIndicator,
                    TextContent = Magics.FontAwesomeListUl };

                var aggregationIndicator = new HTMLSpanElement {
                    ClassName = Magics.CssClassAggregationIndicator,
                    TextContent = "Σ" };
                
                var sortControl = new HTMLSpanElement();
                sortControl.Style.FontFamily = Magics.FontAwesomeName;

                var order = item.Orderable ? SortOrder.Unspecified : SortOrder.Unsupported;
                sortControl.TextContent = order.GetIconText();
                
                _sortControls.Add(sortControl);
                alwaysVisibleItemsContainer.AddEventListener("click", OnSortClicked, false);
                
                SetSortOrder(sortControl, order);
                
                alwaysVisibleItemsContainer.AppendChild(filterIndicator);
                alwaysVisibleItemsContainer.AppendChild(groupIndicator);
                alwaysVisibleItemsContainer.AppendChild(aggregationIndicator);
                alwaysVisibleItemsContainer.AppendChild(item.LabelElem);
                alwaysVisibleItemsContainer.AppendChild(sortControl);
                
                var resizeHandle = new HTMLDivElement {ClassName = Magics.CssClassResizeHandle};
                
                thLabel.AppendChild(alwaysVisibleItemsContainer);
                thLabel.AppendChild(resizeHandle);
                trLabels.AppendChild(thLabel);
                trFilter.AppendChild(thFilter);
            }

            _thead.AppendChild(trLabels);
            HeaderRowCount++;

            _thead.AppendChild(trFilter);
            HeaderRowCount++;
        }
        
        public void DeleteBodyRow(int position) {
            _tbody.RemoveChild(
                _tbody.Children[1+position]
            );
            BodyRowCount--;
        }

        public void InsertBodyRow(int position, IEnumerable<Element> rawItems) {
            var items = rawItems.ToList();
            var tr = new Element(ElementType.TableRow);
            var widths = GetColumnWidths(items.Count);

            var i = 0;
            foreach (var item in items) {
                var td = new HTMLElement(ElementType.TableDataCell);
                var width = widths[i];

                if (width.HasValue) {
                    td.Style.MinWidth = width + "px";
                    td.Style.MaxWidth = width + "px";
                    td.Style.Width = width + "px";
                }

                td.AppendChild(item);
                tr.AppendChild(td);
                i++;
            }

            _tbody.InsertBefore(
                tr, 
                _tbody.ChildNodes[position+1]
            );
            BodyRowCount++;
        }

        public void RowCssClassAdd(int rowIndex, string className) {
            _tbody.GetChildAtOrNull(
                rowIndex
                + 1 //leading row
            ).ClassList.Add(className);
        }

        public void RowCssClassRemove(int rowIndex, string className) {
            _tbody.GetChildAtOrNull(
                rowIndex
                + 1 //leading row
            ).ClassList.Remove(className);
        }

        public static implicit operator RenderElem<HTMLElement>(HtmlTableBasedTableView inp) {
            return RenderElem<HTMLElement>.Create(inp);
        }

        //public void DebugPrintContents(int colNo) {
        //    Logger.Debug(GetType(), "HtmlTableBasedTableView DebugPrintContents() start");
        //    _tbody.ChildNodes.Skip(1).Take(_tbody.ChildNodes.Length-2).ForEachI((i,rawX) => {
        //        var x = (HTMLTableRowElement)rawX;
        //        Logger.Debug(GetType(), "HtmlTableBasedTableView DebugPrintContents() itm value={0}", x.Children[colNo].InnerHTML);
        //    });
        //    Logger.Debug(GetType(), "HtmlTableBasedTableView DebugPrintContents() stop");
        //}
    }
}
