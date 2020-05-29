using System;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class MaskedNumberInputView : IView<HTMLElement> {
        private readonly int _sizeInChars;
        private readonly char _placeholderChar;
        private readonly HTMLDivElement _container;
        private readonly HTMLInputElement _input;
        private bool _ignoreNextFocus;
        private bool _programaticFocus;
        private int _programaticFocusAt;
        private bool _programaticFocusPendingRemoval;
        
        public HTMLElement Widget => _container;
        public HTMLInputElement InputWidget => _input;

        public bool Enabled {
            get => !_input.Disabled;
            set => _input.Disabled = !value;
        }

        public int? Value {
            get {
                if (int.TryParse(_input.Value, out var res) && res >= MinMaxLimits.Item1 && res <= MinMaxLimits.Item2) {
                    return res;
                }
                return null;
            }
            set {
                _input.Value = !value.HasValue ? 
                    new string(_placeholderChar,_sizeInChars) 
                    : 
                    value.Value.ToString(new string(_placeholderChar, _sizeInChars));
                UpdateValidationState();
            } }
        public Tuple<int,int> MinMaxLimits { get; set;}
        public event Action OnChange;
        public event Action<ProcessingOutcome,Action> RequestNavigation;
        public event Action<CalendarState,bool> CalendarRequest; //bool=forced

        public MaskedNumberInputView(
            int sizeInChars, Tuple<int,int> limits, Func<int,char?> processKey, char placeholderChar) {

            _sizeInChars = sizeInChars;
            _placeholderChar = placeholderChar;

            if (limits.Item1.ToString().Length > sizeInChars || limits.Item2.ToString().Length > sizeInChars) {
                throw new Exception("limits may not have consist of more digits than sizeInDigits");
            }
            MinMaxLimits = limits;

            _container = new HTMLDivElement {ClassName = GetType().FullNameWithoutGenerics()};
            
            var upArrow = new HTMLDivElement();
            upArrow.SetAttribute(Magics.AttrDataIcon, FontAwesomeSolid.IconChevronLeft);
            upArrow.AddClasses(Magics.CssClassUpArrow, IconFontType.FontAwesomeSolid.ToCssClassName());
            upArrow.OnClick += ev => {
                if (!Enabled) {
                    return;
                }

                var outcome = ProcessKey(Magics.KeyCodeArrowUp, false);
                
                if (outcome.Changed) {
                    UpdateValidationState();
                    OnChange?.Invoke();
                }
            };

            var downArrow = new HTMLDivElement();
            downArrow.SetAttribute(Magics.AttrDataIcon, FontAwesomeSolid.IconChevronRight);
            downArrow.AddClasses(Magics.CssClassDownArrow, IconFontType.FontAwesomeSolid.ToCssClassName());
            downArrow.OnClick += ev => {
                if (!Enabled) {
                    return;
                }

                var outcome = ProcessKey(Magics.KeyCodeArrowDown, false);
                
                if (outcome.Changed) {
                    UpdateValidationState();
                    OnChange?.Invoke();
                }
            };

            _input = new HTMLInputElement {
                Size = sizeInChars,
                Value = new string(_placeholderChar, sizeInChars) };

            UpdateValidationState();

            _input.OnKeyDown += ev => {
                Logger.Debug(GetType(), "got key {0} and char={1} shift?={2}", 
                    ev.KeyCode, (char)ev.CharCode, ev.ShiftKey);
                
                if (!Enabled) {
                    return;
                }

                var ch = processKey(ev.KeyCode);

                var outcome = ch.HasValue ? 
                    AttemptProcessChar(ev.KeyCode, ch.Value) 
                    : 
                    ProcessKey(ev.KeyCode, ev.ShiftKey);

                Logger.Debug(GetType(), "outcome {0}", outcome);
                
                if (outcome.Cursor.HasValue) {
                    switch (outcome.Cursor.Value) {
                        case Direction.Forward:
                            if (_input.SelectionStart+1 <= _input.Value.Length-1) {
                                _input.SelectionStart++;
                                _input.SelectionEnd = _input.SelectionStart;
                            } else {
                                //if no more fields leave cursor in logical place
                                if (_input.SelectionStart + 1 == _input.Value.Length) {
                                    _input.SelectionStart++;
                                    _input.SelectionEnd = _input.SelectionStart;
                                }
                                outcome.Navigate = Direction.Forward;
                                outcome.NavigateIsProgrammatic = true;
                            }
                            break;

                        case Direction.Backward:
                            if (_input.SelectionStart > 0) {
                                _input.SelectionStart--;
                                _input.SelectionEnd = _input.SelectionStart;
                            } else {
                                outcome.Navigate = Direction.Backward;
                                outcome.NavigateIsProgrammatic = true;
                            }
                            break;
                        
                        default: throw new Exception("unsupported Direction");
                    }
                }
                
                Logger.Debug(GetType(), "OnKeyDown() input selectionStart={0} selectionEnd={1} valueLenght={2} navigate={3} consumed={4}",
                    _input.SelectionStart, _input.SelectionEnd, _input.Value.Length, 
                    outcome.Navigate.HasValue ? outcome.Navigate.Value.ToString() : "", //otherwise it causes bridge bug
                    outcome.Consumed);

                if (outcome.Consumed.GetValueOrDefault()) {
                    ev.PreventDefault();
                }

                if (outcome.Navigate.HasValue) {
                    //calls preventDefault() on successful non programmatic navigation
                    RequestNavigation?.Invoke(outcome, ev.PreventDefault); 
                } 
                
                if (outcome.Changed) {
                    UpdateValidationState();
                    OnChange?.Invoke();
                }
                
                Logger.Debug(GetType(), "OnKeyDown() outcome={0}", outcome);
            };

            _container.AppendChild(upArrow);
            _container.AppendChild(_input);
            _container.AppendChild(downArrow);
            
            _input.OnClick += ev => {
                if (!Enabled) {
                    return;
                }
                CalendarRequest?.Invoke(CalendarState.Show, true);
            };

            _input.OnFocus += ev => {
                _container.RemoveClasses(Magics.CssClassInactive);
                _container.AddClasses(Magics.CssClassActive);
                
                if (_programaticFocus) {
                    _programaticFocus = false;

                    Logger.Debug(GetType(), "got programmatic focus value={0} focusAt={1} pendingKey={2}", 
                        _input.Value, _programaticFocusAt, _programaticFocusPendingRemoval);
                    
                    _input.SelectionStart = _programaticFocusAt;
                    _input.SelectionEnd = _programaticFocusAt;

                    //pending backspace?
                    if (_programaticFocusPendingRemoval && ApplyChar(_placeholderChar, false)) {
                        _input.SelectionStart = _programaticFocusAt - 1;
                        _input.SelectionEnd = _programaticFocusAt - 1;

                        _programaticFocusPendingRemoval = false; //reset
                    } 
                    return;
                }
                
                _input.SelectionStart = 0;
                _input.SelectionEnd = 0;

                if (_ignoreNextFocus) {
                    _ignoreNextFocus = false;
                    return;
                }

                if (!ev.IsUserGenerated()) {
                    //don't advertise it
                    _ignoreNextFocus = true;
                    _input.Focus();
                    return;
                }
                
                CalendarRequest?.Invoke(CalendarState.Show, false);
            };
            
            _input.OnBlur += ev => {
                _container.RemoveClasses(Magics.CssClassActive);
                _container.AddClasses(Magics.CssClassInactive);
            };
        }
        
        private ProcessingOutcome ProcessKey(int keyCode, bool shift) {
            switch (keyCode) {
                case Magics.KeyCodeEnter:
                    return new ProcessingOutcome {Consumed = false};

                case Magics.KeyCodeEscape:
                    CalendarRequest?.Invoke(CalendarState.Hide, true);
                    return new ProcessingOutcome {Consumed = true};
                    
                case Magics.KeyCodeTab:
                    return new ProcessingOutcome {
                        Consumed = null, //don't know yet if navigation happens programatically or auto
                        Navigate = shift ? Direction.Backward : Direction.Forward};
                    
                case Magics.KeyCodeBackspace:
                    var succ = ApplyChar(_placeholderChar, false);
                    return new ProcessingOutcome {
                        Consumed = true,
                        Changed = succ,
                        Cursor = Direction.Backward,
                        PendingCharRemoval = !succ && keyCode == Magics.KeyCodeBackspace };

                case Magics.KeyCodeArrowLeft:
                    return new ProcessingOutcome {
                        Consumed = true,
                        Changed = false,
                        Cursor = Direction.Backward };

                case Magics.KeyCodeArrowRight:
                    return new ProcessingOutcome {
                        Consumed = true,
                        Changed = false,
                        Cursor = Direction.Forward };

                case Magics.KeyCodeArrowDown:
                    var decremented = false;

                    if (!Value.HasValue) {
                        Value = MinMaxLimits.Item2;
                    } else if (Value - 1 >= MinMaxLimits.Item1) {
                        Value--;
                        decremented = true;
                    }

                    return new ProcessingOutcome {
                        Consumed = true,
                        Changed = decremented };

                case Magics.KeyCodeArrowUp:
                    var incremented = false;

                    if (!Value.HasValue) {
                        Value = MinMaxLimits.Item1;
                    } else if (Value + 1 <= MinMaxLimits.Item2) {
                        Value++;
                        incremented = true;
                    }
                    
                    return new ProcessingOutcome {
                        Consumed = true,
                        Changed = incremented };

                default: return new ProcessingOutcome {Consumed = true};
            }
        }

        private ProcessingOutcome AttemptProcessChar(int causedByKeyCode, char ch) {
            var consumed = ApplyChar(ch, true);
            
            return new ProcessingOutcome {
                Consumed = true, //so that rejected characters are really rejected
                Cursor = Direction.Forward,
                Changed = consumed,
                PendingCharRemoval = !consumed && causedByKeyCode == Magics.KeyCodeBackspace
            };
        }

        private bool ApplyChar(char ch, bool afterCursor) {
            Logger.Debug(GetType(), "ApplyChar ch={0} at={1} toRight={2} valueBefore={3}",
                ch, _input.SelectionStart, afterCursor, _input.Value);

            if (afterCursor && _input.SelectionStart >= _input.Value.Length) {
                return false;
            }
                    
            if (!afterCursor && _input.SelectionStart <= 0) {
                return false;
            }

            var curPosCopy = _input.SelectionStart;

            _input.Value = 
                _input.Value.Substring(0, _input.SelectionStart + (afterCursor ? 0 : -1)) + 
                ch +
                _input.Value.Substring(_input.SelectionStart+1 + (afterCursor ? 0 : -1));
                    
            _input.SelectionStart = curPosCopy;
            _input.SelectionEnd = curPosCopy;

            Logger.Debug(GetType(), "ApplyChar applied curPos={0} valueAfter={1}", 
                _input.SelectionStart, _input.Value);

            return true;
        }

        private void UpdateValidationState() {
            _input.AddOrRemoveClass(!Value.HasValue, Magics.CssClassWrongDateComponent);
        }

        public void FocusAtBeginning() {
            _programaticFocus = true;
            _programaticFocusAt = 0;
            _input.Focus();
        }

        public void FocusAtEnding(bool pendingCharRemoval) {
            _programaticFocus = true;
            _programaticFocusAt = _input.Value.Length + (!pendingCharRemoval ? -1 : 0);
            _programaticFocusPendingRemoval = pendingCharRemoval;
            _input.Focus();
        }
    }
}
