using System;
using System.Collections.Generic;
using Bridge;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public abstract class BaseInputView<DataT,ContT> : IReadWriteValueView<HTMLElement,DataT> where ContT : HTMLElement {
        protected readonly ControlWithValueLogic<DataT> _logic;
        protected readonly HTMLSpanElement _container;
        private readonly HTMLLabelElement _label;
        private readonly HTMLSpanElement _textVersion;
        private bool _fromClick;
        private readonly ContT _input;
        private readonly Func<ContT, DataT> _getPhysicalValue;
        private Func<DataT,string> _textVersionProvider;
        private bool _gotEnter,_commitValue;
        private HTMLElement _clearContainer;
        private bool _isClearable = false;
        private readonly HTMLElement _inputContainer;
        private bool _ignoreNextFocus;

        public HTMLElement LabelWidget  => _label;
        public string Label { set { _label.TextContent = value; } }
        public ContT InputWidget => _input;
        public HTMLElement InputContainerWidget => _inputContainer;

        public bool RaisesChangedOnKeyPressed { get; set; } = true;
        public bool SubmitOnEnter { get; set; } = true;

        /// <summary>
        /// portable version of ::-ms-clear
        /// </summary>
        public bool Clearable {
            set {
                _container.AddOrRemoveClass(value, Magics.CssClassIsClearable); 

                if (value && _isClearable || !value && !_isClearable) {
                    return; //same state / no change
                }

                if (value) {
                    EnableClearer();
                } else {
                    DisableClearer();    
                }
            }
        }

        public event UiErrorsUpdated ErrorsChanged;
        public event ValueChangedSimple<DataT> Changed;
        public event Action<Event> Focused;
        public event Action<Event> Clicked;
        public event Action Blurred;

        public HTMLElement Widget => _container;

        public bool IsValidating { 
            get => _container.ClassList.Contains(Magics.CssClassIsValidating);
            set => _container.AddOrRemoveClass(value, Magics.CssClassIsValidating);
        }
        
        public DataT Value { 
            get => _logic.Value;
            set => _logic.Value = value;
        }

        private bool IsClickToEdit => _textVersionProvider != null;
        public abstract bool Enabled { protected get; set;}
        public ISet<string> DisabledReasons { set => DefaultInputLogic.SetDisabledReasons(_input, value); }

        public ISet<string> Errors => DefaultInputLogic.GetErrors(_input);
        public void SetErrors(ISet<string> errors, bool isUserInput) { 
            DefaultInputLogic.SetErrors(_container, _input, isUserInput, errors);
            ErrorsChanged?.Invoke(this, errors);
        }
        
        protected BaseInputView(
            ContT input, string containerClass, Func<ContT,DataT> getPhysicalValue, 
            Action<ContT,DataT> setPhysicalValue, string label) {

            var id = UniqueIdGenerator.GenerateAsString();
            
            _input = input;
            _getPhysicalValue = getPhysicalValue;
            _input.SetAttribute("id", id);

            _inputContainer = new HTMLDivElement();
            _inputContainer.ClassName = Magics.CssClassInputContainer;

            _textVersion = new HTMLSpanElement();
            _textVersion.ClassList.Add(Magics.CssClassTextVersion);
            
            _label = new HTMLLabelElement {
                TextContent = label
            };
            _label.SetAttribute("for", id);
			
            _container = new HTMLSpanElement();
            _container.ClassName = containerClass;
            _container.AppendChild(_label);
            
            _inputContainer.AppendChild(_textVersion);
            _inputContainer.AppendChild(_input);
            
            _container.AppendChild(_inputContainer);
            
            BuildClearer();
            DisableClearer();

            _input.OnFocus += ev => {
                if (_ignoreNextFocus) {
                    _ignoreNextFocus = false;
                    return;
                }

                if (!_fromClick) {
                    _input.SelectWholeTextAndMoveCursorToEnd();
                }
                _fromClick = false;
                Focused?.Invoke(ev);

                if (!_input.HasFocus()) {
                    //non user generated (isTrusted == false) events don't invoke default action in Chrome 
                    _ignoreNextFocus = true;
                    _input.Focus();  
                }
            };

            _input.OnBlur += ev => {
                Blurred?.Invoke();
            };

            _input.OnClick += ev => {
                _fromClick = true;
                Clicked?.Invoke(ev);
            };

            _label.OnClick += _ => {
                if (!IsClickToEdit) {
                    return;
                }

                _textVersion.Click();
            };

            _logic = new ControlWithValueLogic<DataT>( 
                (newVal,isUser) =>{
                    Changed?.Invoke(newVal, isUser);
                    if (_gotEnter) {
                        _gotEnter = false;
                        Logger.Debug(GetType(), "_gotEnter is true so will invoke default form button");
                        _input.ActivateMyFormsDefaultButtonIfAny();
                    }
                },
                () => getPhysicalValue(_input),
                v => {
                    setPhysicalValue(_input, v);
                    if (IsClickToEdit) {
                        _textVersion.TextContent = _textVersionProvider(v);    
                    }
                    UpdateClearActionVisibility();
                },
                () => Enabled,
                v => Enabled = v,
                () => IsValidating,
                v => IsValidating = v
            );
            UpdateClearActionVisibility(); //initial value

            _input.OnChange += ev => {
                Logger.Debug(GetType(), "got input onChange cur _logic.value={0} _physicalValue={1} isFromUser={2}", 
                    _logic.Value, _getPhysicalValue(_input), ev.IsUserGenerated());
                _logic.PhysicalChanged(false, ev.IsUserGenerated());
                UpdateClearActionVisibility();
            };
            
            //onkeydown is called before oninput
            _input.OnKeyDown += ev => {
                var isNavigation = ev.IsNavigationKey();
 
                Logger.Debug(GetType(), "got onkeyup {0} enabled?={1} isValidating={2} isNavigation={3} RaisesChangedOnKeyPressed={4} _commitChange={5} SubmitOnEnter={6}", 
                    ev.KeyCode, Enabled, IsValidating, isNavigation, RaisesChangedOnKeyPressed, _commitValue, SubmitOnEnter);
                
                //submit via ENTER works no matter the disabled/enabled
                _gotEnter = ev.KeyCode == Magics.KeyCodeEnter;
                if (ev.KeyCode == Magics.KeyCodeEnter && SubmitOnEnter) {
                    _logic.PhysicalChanged(true, ev.IsUserGenerated());
                }
            };

            //input when character is really added to control
            //https://www.quirksmode.org/dom/events/keys.html

            _input.OnInput += rawEv => {
                var value = getPhysicalValue(_input);
                _commitValue = RaisesChangedOnKeyPressed || _gotEnter;

                Logger.Debug(GetType(), "got event input enabled?={0} isValidating={1} RaisesChangedOnKeyPressed={2} physicalValue={3} _commitChange={4} isFromUser={5}", 
                    Enabled, IsValidating, RaisesChangedOnKeyPressed, value, _commitValue, rawEv.IsUserGenerated());

                if (IsClickToEdit) {
                    _textVersion.TextContent = _textVersionProvider(value);
                }

                if (_commitValue) {
                    _logic.PhysicalChanged(false, rawEv.IsUserGenerated());    
                }
                
                UpdateClearActionVisibility();
            };

            _textVersion.OnClick += ev => {
                _container.ClassList.Remove(Magics.CssClassViewing);
                _container.ClassList.Add(Magics.CssClassEditing);
                _input.Focus();
            };
            
            _input.OnClick += x => {
                if (!IsClickToEdit) {
                    return;
                }

                //checkboxes mostly are not mutated via keyup 
                _textVersion.TextContent = _textVersionProvider(getPhysicalValue(_input));
            };

            DocumentUtil.AddMouseDownListener(_input, x => {
                if (!IsClickToEdit) {
                    return;
                }

                if (!x.HasHtmlTarget()) {
                    return;
                }
                var htmlTarget = x.HtmlTarget();
                
                if (htmlTarget.IsElementOrItsDescendant(_inputContainer)) {
                    //clicked inside control (focus stays within logical control) thus do nothing
                    return;
                }

                _container.ClassList.Remove(Magics.CssClassEditing);
                _container.ClassList.Add(Magics.CssClassViewing);
            });
        }
        
        public void EnableClickToEdit() {
            EnableClickToEdit(x => x == null ? "" : x.ToString());
        }

        public void EnableClickToEdit(Func<DataT,string> textVersionProvider) {
            _textVersionProvider = textVersionProvider;

            _container.ClassList.Add(Magics.CssClassClickToEditable);
            _container.ClassList.Add(Magics.CssClassViewing);
            _textVersion.TextContent = _textVersionProvider(_logic.Value);
        }

        private void EnableClearer() {
            Script.Set(_clearContainer.Style, "display", "");
        }

        private void DisableClearer() {
            _clearContainer.Style.Display = Display.None;
        }

        private void BuildClearer() {
            _clearContainer = new HTMLSpanElement();
            _clearContainer.ClassName = Magics.CssClassClearContainer;
            _clearContainer.OnClick += _ => {
                _logic.Value = default(DataT);
                _logic.PhysicalChanged(false, true);
                UpdateClearActionVisibility();
            };

            _inputContainer.InsertAfter(_clearContainer, _input);
        }

        private void UpdateClearActionVisibility() {
            //clear action is for controls that can have 'empty' value
            var isEmpty = _logic.Value == null || _logic.Value is string && string.IsNullOrWhiteSpace(_logic.Value as string);
            _container.AddOrRemoveClass(isEmpty, Magics.CssClassIsEmpty);
        }
        
        public static implicit operator RenderElem<HTMLElement>(BaseInputView<DataT,ContT> inp) {
            return RenderElem<HTMLElement>.Create(inp);
        }
    }
}
