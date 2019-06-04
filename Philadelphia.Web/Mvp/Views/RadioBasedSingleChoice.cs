using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public delegate void RadioBasedSingleChoiceItemAdder(
        object ctxFromBeforeAddItems, 
        HTMLElement container, 
        Tuple<string, string> viewValue, 
        HTMLInputElement inputElem,
        HTMLLabelElement labelElem, 
        int itemNo);

    public class RadioBasedSingleChoice : IRestrictedSingleReadWriteValueView<HTMLElement,Tuple<string, string>> {
        private readonly string _uniqueNumberAsName;
        private readonly HTMLLabelElement _genericLabelOrNull;
        private readonly HTMLDivElement _container;
        private readonly ControlWithValueLogic<Tuple<string, string>> _logic;
        private readonly Dictionary<string,HTMLInputElement> _valueToItem = new Dictionary<string, HTMLInputElement>();
        private readonly Dictionary<string,string> _valueToLabel = new Dictionary<string, string>();

        public HTMLElement Widget => _container;
        public event ValueChangedSimple<Tuple<string, string>> Changed;
        public event UiErrorsUpdated ErrorsChanged;

        public bool IsValidating {
            get { return _logic.IsValidating; }
            set { _logic.IsValidating = value; }
        }

        public bool Enabled { 
            get { return _logic.Enabled; }
            set { _logic.Enabled = value; } 
        }

        /// <summary>
        /// context from BeforeAddItems, container, raw value, item as element, label as element, item number
        /// </summary>
        private RadioBasedSingleChoiceItemAdder ItemAdder {get; }
        
        /// <summary>
        /// container, items count. returns context to be passed to ItemAdder
        /// </summary>
        public Func<HTMLElement, int, object> BeforeAddItems {private get; set;}
        
        public IEnumerable<Tuple<string, string>> PermittedValues {
            set {
                var valToPreserve = Value?.Item1;

                Logger.Debug(GetType(), "replacing PermittedValues while preserving value={0}", valToPreserve);
                _container.RemoveAllChildren();
                if (_genericLabelOrNull != null) {
                    _container.AppendChild(_genericLabelOrNull);
                }
                
                _valueToItem.Clear();
                _valueToLabel.Clear();

                var itemNo = 0;

                var ctx = BeforeAddItems(_container, value.Count());

                value.ForEach(x => {
                    var itemId = UniqueIdGenerator.GenerateAsString();

                    var item = new HTMLInputElement {
                        Type = InputType.Radio,
                        Id = itemId,
                        Value = x.Item1,
                        Name = _uniqueNumberAsName,
                        Checked = x.Item1 == valToPreserve
                    };
                    
                    _valueToItem.Add(x.Item1, item);
                    _valueToLabel[x.Item1] = x.Item2;

                    var lbl = new HTMLLabelElement {
                        TextContent = x.Item2,
                        HtmlFor = itemId };
                    
                    item.OnChange += ev => {
                        Logger.Debug(GetType(), "item->OnChange()");
                        _logic.PhysicalChanged(false, ev.IsUserGenerated());
                    };
                    
                    ItemAdder(ctx, _container, x, item, lbl, itemNo);
                    itemNo++;
                });
            }
        }
        
        public ISet<string> DisabledReasons { 
            set { DefaultInputLogic.SetDisabledReasons(_container, value);} 
        }
        
        public ISet<string> Errors => DefaultInputLogic.GetErrors(_container);

        public void SetErrors(ISet<string> errors, bool isUserInput) { 
            DefaultInputLogic.SetErrors(_container, _container, isUserInput, errors);
            ErrorsChanged?.Invoke(this, errors);
        }
        
        public Tuple<string,string> Value { 
            get { return _logic.Value;} 
            set { _logic.Value = value;}
        }
        
        public RadioBasedSingleChoice(string labelOrNull = null, RadioBasedSingleChoiceItemAdder customItemAdder = null) {
            BeforeAddItems = (element, i) => null;

            ItemAdder = customItemAdder ?? ((ctx, cntnr, rawItem, itemElem, lblElem, itemNo) => {
                cntnr.AppendChild(itemElem);
                cntnr.AppendChild(lblElem); });

            _uniqueNumberAsName = UniqueIdGenerator.GenerateAsString();

            _container = new HTMLDivElement();
            _container.ClassName = GetType().FullNameWithoutGenerics();

            if (labelOrNull != null) {
                _genericLabelOrNull = new HTMLLabelElement {TextContent = labelOrNull};
                _container.AppendChild(_genericLabelOrNull);                
            }
            
            _logic = new ControlWithValueLogic<Tuple<string,string>>( 
                (newVal,isUser) => Changed?.Invoke(newVal, isUser),
                () => {
                    var active = _valueToItem.Values.FirstOrDefault(x => x.Checked);
                    return active == null ? Tuple.Create("", "") : Tuple.Create(active.Value, _valueToLabel[active.Value]);
                },
                v => {
                    var emptying = v == null || string.IsNullOrEmpty(v.Item1);

                    Logger.Debug(GetType(), "setPhysicalValue emptying?={0} value=({1};{2})", 
                        emptying, v?.Item1, v?.Item2);

                    if (emptying) {
                        foreach (var x in _valueToItem.Values) {
                            if (x.Checked) {
                                x.Checked = false;
                                break;
                            }
                        }
                        return;
                    }

                    _valueToItem[v.Item1].Checked = true;
                },
                () => _valueToItem.Any() && !_valueToItem.First().Value.Disabled,
                v => _valueToItem.Values.ForEach(x => _valueToItem.First().Value.Disabled = !v),
                () => _container.ClassList.Contains(Magics.CssClassIsValidating),
                v => _container.AddOrRemoveClass(v, Magics.CssClassIsValidating)
            );
            _logic.ValueToString = x => x == null ? "<Tuple null>" : $"<Tuple fst={x.Item1} snd={x.Item2}>";
        }
        
        public static implicit operator RenderElem<HTMLElement>(RadioBasedSingleChoice inp) {
            return RenderElem<HTMLElement>.Create(inp);
        }
    }
}
