﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bridge;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public enum AutocompleteSrcType {
        KeyDown,
        KeyPressed
    }

    public class AutocompleteDropdown<DataT> : IReadWriteValueView<HTMLElement,DataT> {
        private Func<int,string,Task<DataT[]>> _matchingValuesProvider;
        private Func<DataT, string> _extractLabel;
        private Func<string, DataT, bool> _isCompleteValue;

        private readonly HTMLDivElement _cnt = new HTMLDivElement();
        private readonly HTMLInputElement _input = new HTMLInputElement()
            .With(x => x.Id = UniqueIdGenerator.GenerateAsString());
        private readonly HTMLDivElement _options = new HTMLDivElement()
            .With(x => x.AddClasses(Magics.CssClassOptions));
        private int _requestCount;
        private int? _activeItemNo;
        private string _valueBeforeArrows;
        private List<DataT> _availOptions;
        private DataT _value;
        private bool _ignoreNextFocus, _logicalIsValidating;
        private TextType _textType;
        private int _delayMilisec;
        private string _matchingValuesKey;
        
        public HTMLElement Widget => _cnt;
        public int MaxVisibleItems {get; set; } = 10;
        public event ValueChangedSimple<DataT> Changed;
        public event UiErrorsUpdated ErrorsChanged;
        public HTMLInputElement InputElem => _input;

        public DataT Value {
            get { return _value; }
            set {
                _value = value; 
                _input.Value = value != null ? _extractLabel(value) : "";
                _options.RemoveAllChildren();
            }
        }
        public ISet<string> DisabledReasons { set { DefaultInputLogic.SetDisabledReasons(_input, value);} }
        public ISet<string> Errors => DefaultInputLogic.GetErrors(_input);
        public bool OptionsVisible => _options.ChildElementCount > 0;

        //not really disabling field as it looses focus...
        public bool IsValidating {
            get { return _logicalIsValidating; }
            set {
                _logicalIsValidating = value;
                _cnt.AddOrRemoveClass(value || _requestCount > 0, Magics.CssClassIsValidating);
            }
        }
        public bool Enabled { 
            get { return !_input.ClassList.Contains(Magics.CssClassDisabled); }
            set { 
                _cnt.AddOrRemoveClass(!value, Magics.CssClassDisabled);
                _input.AddOrRemoveClass(!value, Magics.CssClassDisabled);
            } 
        }
        public void SetErrors(ISet<string> errors, bool isUserInput) { 
            DefaultInputLogic.SetErrors(_cnt, _input, isUserInput, errors);
            ErrorsChanged?.Invoke(this, errors);
        }
        public List<DataT> AvailableOptions { set {
            _options.RemoveAllChildren();
            value
                .Select(x => {
                    var opt = new HTMLDivElement();

                    switch (_textType) {
                        case TextType.TreatAsText:
                            opt.TextContent = _extractLabel(x);
                            break;

                        case TextType.TreatAsHtml:
                            opt.InnerHTML = _extractLabel(x);
                            break;

                        case TextType.TreatAsPreformatted:
                            opt.Style.WhiteSpace = WhiteSpace.Pre;
                            opt.TextContent = _extractLabel(x);
                            break;

                        default: throw new Exception("unsupported TextType");
                    }

                    opt.OnClick += ev => {
                        Logger.Debug(GetType(), "mouse click choose value key={0}", x);
                        _input.Value = _extractLabel(x);
                        _value = x;
                        Changed?.Invoke(Value, true);
                        HideOptions();
                    };

                    return opt;
                }).ForEach(x => _options.AppendChild(x));
        } }

        public AutocompleteDropdown(
                string label,
                TextType textType = TextType.TreatAsText, 
                int delayMilisec = Magics.AutocompleteDefaultDelay) {

            _cnt.ClassName = GetType().FullNameWithoutGenerics();
            
            var lbl = new HTMLLabelElement {HtmlFor = _input.Id, TextContent = label};
            _cnt.AppendChild(lbl);

            _cnt.AppendChild(_input);
            _cnt.AppendChild(_options);
            HideOptions();

            DocumentUtil.AddMouseDownListener(_cnt, x => {
                if (!x.HasHtmlTarget()) {
                    return;
                }
                var htmlTarget = x.HtmlTarget();

                if (htmlTarget.IsElementOrItsDescendant(_cnt)) {
                    //clicked inside control (focus stays within logical control) thus do nothing
                    return;
                }

                HideOptions();
            });

            _input.OnFocus += ev => {
                if (_ignoreNextFocus) {
                    _ignoreNextFocus = false;
                    return;
                }

                ShowOptions();
                
                if (!_input.HasFocus()) {
                    //non user generated (isTrusted == false) events don't invoke default action in Chrome 
                    _ignoreNextFocus = true;
                    _input.Focus();  
                }
            };
            _input.OnKeyDown += ev => {
                switch (ev.KeyCode) {
                    case Magics.KeyCodeEscape:
                        if (OptionsVisible) {
                            ev.PreventDefault();
                            ev.StopPropagation();
                            HideOptions();    
                        }
                        break;

                    case Magics.KeyCodeEnter:
                    case Magics.KeyCodeArrowUp:
                    case Magics.KeyCodeArrowDown:
                        ev.PreventDefault();
                        OnKeyboardEvent(AutocompleteSrcType.KeyDown, ev);
                        break;

                    case Magics.KeyCodeBackspace:
                        OnKeyboardEvent(AutocompleteSrcType.KeyDown, ev); //it is not called onkeypress
                        break;

                    case Magics.KeyCodeTab:
                        HideOptions();
                        break;

                    default: break;
                }                
            };
            _input.OnKeyPress += ev => {
                switch (ev.KeyCode) {
                    case Magics.KeyCodeBackspace:
                    case Magics.KeyCodeEnter:
                    case Magics.KeyCodeArrowUp:
                    case Magics.KeyCodeArrowDown:
                        ev.PreventDefault();
                        break;

                    default:
                        OnKeyboardEvent(AutocompleteSrcType.KeyPressed, ev);
                        break;
                }
            };

            _textType = textType;
            _delayMilisec = delayMilisec;
        }

        public void Configure(
                Func<int, string, Task<DataT[]>> matchingValuesProvider,
                Func<DataT, string> extractLabel,
                Func<string, DataT, bool> isCompleteValue) {

            _matchingValuesProvider = matchingValuesProvider;
            _extractLabel = extractLabel;
            _isCompleteValue = isCompleteValue;
        }

        private void ShowOptions() {
            _options.Style.SetProperty("display", "");
        }

        private void HideOptions() {
            _options.Style.Display = Display.None;
        }

        private void ActivateItem(int idx) {
            ((HTMLElement)_options.ChildNodes[idx]).AddClasses(Magics.CssClassActive);
        }

        private void DeactivateItem(int idx) {
            ((HTMLElement)_options.ChildNodes[idx]).RemoveClasses(Magics.CssClassActive);
        }

        private void ChangeInputValueTo(string val) {
            _input.Value = val;
        }

        private void OnKeyboardEvent(
                AutocompleteSrcType src, KeyboardEvent<HTMLInputElement> ev) {

            Logger.Debug(GetType(), "autocomplete input={0} key={1} src={2}", _input.Value, ev.KeyCode, src);

            if (!ev.IsUserGenerated()) {
                return;
            }

            if (src == AutocompleteSrcType.KeyDown) {
                switch (ev.KeyCode) {
                    case Magics.KeyCodeEnter:
                        var changed = false;

                        if (!_activeItemNo.HasValue) {
                            if (_input.Value.Length <= 0) {
                                _value = default(DataT);
                                changed = true;
                            } else if (_availOptions.Count == 1 && _isCompleteValue(_input.Value, _availOptions[0])) {
                                _value = _availOptions[0];
                                changed = true;
                            }
                        } else {
                            var act = _availOptions[_activeItemNo.Value];
                            if (_isCompleteValue(_input.Value, act)) {
                                _value = act;
                                changed = true;
                            }
                        }

                        if (changed) {
                            HideOptions();
                            Changed?.Invoke(Value, true);
                        }
                        break;

                    case Magics.KeyCodeBackspace:
                        ScheduleAutocomplete();
                        break;

                    case Magics.KeyCodeArrowUp:
                        if (!_activeItemNo.HasValue) {
                            break;
                        }
                        
                        DeactivateItem(_activeItemNo.Value);
                        
                        _activeItemNo--;
                        if (_activeItemNo.Value < 0) {
                            ChangeInputValueTo(_valueBeforeArrows);
                            _activeItemNo = null;
                            break;
                        }

                        if (_activeItemNo <= _options.ChildElementCount-1) {
                            ActivateItem(_activeItemNo.Value);
                            ChangeInputValueTo(_options.ChildNodes[_activeItemNo.Value].TextContent);
                        }
                        break;

                    case Magics.KeyCodeArrowDown:
                        if (_activeItemNo.HasValue) {
                            DeactivateItem(_activeItemNo.Value);
                        }

                        if (!_activeItemNo.HasValue) {
                            if (_options.ChildElementCount < 0) {
                                break;    
                            }
                            _valueBeforeArrows = _input.Value;
                            _activeItemNo = 0;
                        } else if (_activeItemNo+1 <= _options.ChildElementCount-1) {    
                            _activeItemNo++;
                        }

                        ActivateItem(_activeItemNo.Value);
                        ChangeInputValueTo(_options.ChildNodes[_activeItemNo.Value].TextContent);
                        break;
                }
                return;
            }
            
            if (src == AutocompleteSrcType.KeyPressed) {
                ScheduleAutocomplete();
                return;
            }
        }

        private async Task<ResultHolder<DataT[]>> FetchMatching() {
            var matchingValuesKey = Guid.NewGuid().ToString();
            _matchingValuesKey = matchingValuesKey;
            var par = _input.Value;

            await Task.Delay(_delayMilisec);
            var mayUse = _matchingValuesKey == matchingValuesKey;

            Logger.Debug(GetType(),
                "autocomplete awaiting for result of input {0} requestCount={1} mayUse={2}",
                par, _requestCount, mayUse);
        
            if (!mayUse) {
                return ResultHolder<DataT[]>.CreateFailure("autocomplete[1] skipping due to new input");
            }

            DataT[] result;

            try {
                result = await _matchingValuesProvider(MaxVisibleItems, par);
            } catch (Exception ex) {
                return ResultHolder<DataT[]>.CreateFailure("autocomplete result not received", ex);
            }

            mayUse = _matchingValuesKey == matchingValuesKey;

            Logger.Debug(GetType(),
                "autocomplete result received itemsCount={0} for input={1} currentInputIs={2} mayUse?={3}",
                result.Length, par, _input.Value, mayUse);

            if (!mayUse) {
                return ResultHolder<DataT[]>.CreateFailure("autocomplete[2] skipping due to new input");
            }
            
            return ResultHolder<DataT[]>.CreateSuccess(result);
        }

        private void ScheduleAutocomplete() {
            if (_matchingValuesProvider == null) {
                return;
            }
            
            Window.SetTimeout(async () => {
                _requestCount++;
                IsValidating = IsValidating;
                
                var res = await FetchMatching();
                if (!res.Success) {
                    Logger.Debug(GetType(), res.ErrorMessage);
                } else {
                    _activeItemNo = null;
                    _availOptions = res.Result.ToList();
                    AvailableOptions = _availOptions;
                }

                _requestCount--;
                IsValidating = IsValidating;
                Logger.Debug(GetType(), "ScheduleAutocomplete ending _requestCount={0}", _requestCount);
            });
        }
        
        public static implicit operator RenderElem<HTMLElement>(AutocompleteDropdown<DataT> self) {
            return RenderElem<HTMLElement>.Create(self);
        }
    }
}
