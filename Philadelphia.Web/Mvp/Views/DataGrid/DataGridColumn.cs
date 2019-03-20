using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class DataGridColumn<RecordT,DataT> : IDataGridColumn<RecordT> where RecordT : new() {
        private readonly IReadOnlyValue<string> _headerLabel;
        private readonly TextType _textMode;
        private readonly Func<DataT,string> _asTextValue;
        private readonly Action<DataT, ICellValueExporter> _exporter;
        private readonly Func<ITransformationMediator, Tuple<HTMLElement, DataGridColumnControllerResult<DataT>>> _transformation;
        private readonly Func<RecordT, DataT> _getValue;
        private readonly ITypedSubscribeable<RecordT> _itemObserverOrNull;
        private readonly Func<IReadWriteValueView<HTMLElement,DataT>> _buildEditorOrNull;
        private readonly Action<RecordT, DataT> _setValueOrNull;
        private readonly IDictionary<RecordT,Action> _subscriptions = new Dictionary<RecordT, Action>();
        
        private double? _computedWidth;
        private double? _forcedWidth;
        private readonly Func<int, DataT, Task<RecordT>> _saveOperationOrNull;
        private readonly Func<RecordT,int> _extractIdOrNull;
        private readonly Func<HTMLElement> _elementInsteadOfFilterOrNull;
        private readonly Action<DataGridModel<RecordT>> _onModelAttachedOrNull;       
        private HTMLElement _curColumnHeaderController;
        private DataGridColumnControllerResult<DataT> _curTransforms;
        private Action<IOrderByCollector<RecordT>,SortOrder> _sorterOrNull;

        private double _minWidth = -1;
        public bool IsSelectionHandler {get; }
        public Action<IWhereCollector<RecordT>> CurrentFilterImpl {get; private set; }
        public Func<IEnumerable<RecordT>, string> CurrentAggregation { get; private set; }
        public Func<IEnumerable<RecordT>, IEnumerable<GroupDescr>> CurrentGrouping { get; private set; }

        public bool IsGrouped => _curTransforms?.IsGroupingActive?.Invoke() ?? false;

        public double? MinimumWidth {
            get => _minWidth;
            set => _minWidth = value.HasValue && value.Value>=0 ? value.Value : -1;
        }

        public string Name => _headerLabel.Value;

        public double Width {
            get { return new []{ _forcedWidth,_computedWidth}.FirstOrDefault(x=>x.HasValue) ?? Magics.DefaultColumnWidthPx;}
            set => _forcedWidth = value;
        }
        
        private bool ObservesModel => _itemObserverOrNull != null;
        private bool IsEditable => _setValueOrNull != null;
        private bool IsSaveable => _saveOperationOrNull != null;

        public double ComputedWidth {set => _computedWidth = Math.Max(value+Magics.ExtraWidthAddedToPrototypePx, _minWidth); } 
        public bool Orderable => _sorterOrNull != null;

        public DataGridColumn(
                IReadOnlyValue<string> headerLabel, 
                TextType textMode,
                Func<RecordT,DataT> getValue, 
                Func<DataT,string> asTextValue, 
                Action<DataT,ICellValueExporter> exporter,
                Func<ITransformationMediator,Tuple<HTMLElement,DataGridColumnControllerResult<DataT>>> transformation,
                ITypedSubscribeable<RecordT> itemObserverOrNull,
                Func<IReadWriteValueView<HTMLElement,DataT>> buildEditorOrNull,
                Action<RecordT,DataT> setValueOrNull,
                Func<int, DataT, Task<RecordT>> saveOperationOrNull,
                Func<RecordT,int> extractIdOrNull,
                bool isSelectionHandler,
                Func<HTMLElement> elementInsteadOfFilterOrNull,
                Action<DataGridModel<RecordT>> onModelAttachedOrNull) {
            
            _headerLabel = headerLabel;
            _textMode = textMode;
            _asTextValue = asTextValue;
            _exporter = exporter;
            _transformation = transformation;
            _getValue = getValue;
            _itemObserverOrNull = itemObserverOrNull;
            _buildEditorOrNull = buildEditorOrNull;
            _setValueOrNull = setValueOrNull;
            _saveOperationOrNull = saveOperationOrNull;
            _extractIdOrNull = extractIdOrNull;
            _elementInsteadOfFilterOrNull = elementInsteadOfFilterOrNull;
            _onModelAttachedOrNull = onModelAttachedOrNull;
            IsSelectionHandler = isSelectionHandler;
        }
        
        public string TextValueFor(RecordT item) {
            return _asTextValue(_getValue(item));
        }

        public void ExportValue(RecordT from, ICellValueExporter into) {
            _exporter(_getValue(from), into);
        }

        public void SortingRuleApply(IOrderByCollector<RecordT> collector, SortOrder sortBy) => _sorterOrNull?.Invoke(collector, sortBy);

        public Element CreateHeaderLabel() {
            var view = new LabellessReadOnlyView();
            view.BindReadOnlyAndInitialize(_headerLabel);
            return view.Widget;
        }
        
        public HTMLElement CreateColumnHeaderController(ITransformationMediator listener) {
            if (_elementInsteadOfFilterOrNull != null) {
                return _elementInsteadOfFilterOrNull();
            }
            var res = _transformation(listener);
            _curColumnHeaderController = res.Item1;
            _curTransforms = res.Item2;

            _sorterOrNull = null;
            if (_curTransforms.SortingImpl != null) {
                _sorterOrNull = (collector, order) => 
                    collector.AddOrderByRule(
                        _getValue, 
                        _curTransforms.SortingImpl.Reorder(order));                
            }

            CurrentFilterImpl = null;
            if (_curTransforms.FilterImpl != null) {
                CurrentFilterImpl = z => z.AddWhereRule(_getValue, _curTransforms.FilterImpl);
            }
   
            CurrentGrouping = null;
            if (_curTransforms.GroupingImpl != null) {
                CurrentGrouping = x => _curTransforms.GroupingImpl(x.Select(_getValue));
            }

            CurrentAggregation = null;
            if (_curTransforms.AggregationImpl != null) {
                CurrentAggregation = x => _curTransforms.AggregationImpl(x.Select(_getValue));
            }

            return _curColumnHeaderController;
        }

        private Element CreateAndInitViewForNonEditable(RecordT item) {
            Logger.Debug(GetType(), "CreateAndInitViewFor subscribing readable {0}", item);

            var elem = new HTMLSpanElement();
            Action listener;

            switch (_textMode) {
                case TextType.TreatAsPreformatted: 
                    elem.Style.WhiteSpace = WhiteSpace.Pre;
                    elem.TextContent = _asTextValue(_getValue(item));
                    listener = () => elem.TextContent = _asTextValue(_getValue(item));
                    break;

                case TextType.TreatAsText:
                    elem.TextContent = _asTextValue(_getValue(item));
                    listener = () => elem.TextContent = _asTextValue(_getValue(item));
                    break;

                case TextType.TreatAsHtml:
                    elem.InnerHTML = _asTextValue(_getValue(item));
                    listener = () => elem.InnerHTML = _asTextValue(_getValue(item));
                    break;

                default: throw new Exception("unsupported textMode");
            }
            
            _itemObserverOrNull.Subscribe(item, listener);
            _subscriptions.Add(item, listener);
            
            Logger.Debug(GetType(), "CreateAndInitViewFor subscribed readable");
            return elem;
        }

        private Element CreateAndInitViewForNonObservable(RecordT item) {
            var result = new HTMLSpanElement();
            
            switch (_textMode) {
                case TextType.TreatAsPreformatted:
                    result.Style.WhiteSpace = WhiteSpace.Pre;
                    result.TextContent = _asTextValue(_getValue(item));
                    break;

                case TextType.TreatAsText:
                    result.TextContent = _asTextValue(_getValue(item));
                    break;

                case TextType.TreatAsHtml:
                    result.InnerHTML = _asTextValue(_getValue(item));
                    break;
                    
                default: throw new Exception("unsupported textMode");
            }

            return result;
        }

        private void SubscribeToPropertyChanges(IReadWriteValue<DataT> model, RecordT item) {
            void Listener() => model.DoChange(_getValue(item), false, this, false);
            _itemObserverOrNull.Subscribe(item, Listener);
            _subscriptions.Add(item, Listener);            
        }

        private Element CreateAndInitViewForSaveable(RecordT item) {
            Logger.Debug(GetType(), "CreateAndInitViewFor subscribing saveable {0}", item);
            
            var model = new AdaptedRemoteValueMutator<DataT,RecordT>(
                () => _getValue(item), 
                x => _setValueOrNull(item, x),
                x => _saveOperationOrNull(_extractIdOrNull(item), x) );
            
            var view = _buildEditorOrNull();
            
            try {
                model.RemoteCallingEnabled = false;
                view.BindReadWriteAndInitialize(model);    
            } finally {
                model.RemoteCallingEnabled = true;
            }

            SubscribeToPropertyChanges(model, item);
            
            Logger.Debug(GetType(), "CreateAndInitViewFor subscribed saveable");
            return view.Widget;
        }
        
        private Element CreateAndInitViewForNonSaveable(RecordT item) {
            Logger.Debug(GetType(), "CreateAndInitViewFor subscribing editable {0}", item);
            
            var model = new AdaptedLocalValue<DataT>(
                () => _getValue(item), 
                x => _setValueOrNull(item, x));

            var view = _buildEditorOrNull();
            view.BindReadWriteAndInitialize(model);    
            
            SubscribeToPropertyChanges(model, item);
            
            Logger.Debug(GetType(), "CreateAndInitViewFor subscribed editable");
            return view.Widget;
        }

        public Element CreateAndInitViewFor(RecordT item) {
            if (ObservesModel && IsEditable) {
                return IsSaveable ? CreateAndInitViewForSaveable(item) : CreateAndInitViewForNonSaveable(item);
            }
            
            if (ObservesModel) {
                return CreateAndInitViewForNonEditable(item);
            }

            return CreateAndInitViewForNonObservable(item);
        }

        public void DeleteViewFor(RecordT item) {
            // no events attached so no need to do any cleanups
            if (ObservesModel) {
                Logger.Debug(GetType(), "CreateAndInitViewFor unsubscribing {0}", item);
                
                _itemObserverOrNull.Unsubscribe(item, _subscriptions[item]);
                _subscriptions.Remove(item);

                Logger.Debug(GetType(), "CreateAndInitViewFor unsubscribed");
            }
        }

        public void OnModelAttached(DataGridModel<RecordT> dgmodel) {
            _onModelAttachedOrNull?.Invoke(dgmodel);
        }
    }
}
