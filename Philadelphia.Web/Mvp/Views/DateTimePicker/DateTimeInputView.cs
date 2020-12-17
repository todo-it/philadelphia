using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class DateTimeInputView : IView<HTMLElement> {
        private readonly DateTimeFormat _precision;

        // REVIEW: inline local only fields
        private readonly HTMLElement _inputsContainer,_validationOutcome;
        private readonly MaskedNumberInputView _yearInput,_monthInput,_dayInput,_hourInput,_minuteInput,_secondInput;
        private readonly InputTypeButtonActionView _activateCalendar,_clearContent;
        private DateTime? _value;
        private readonly IDateTimeBuilder _dateTimeBuilder;

        public event ValueChangedSimple<DateTime?> Changed;
        public event Action<CalendarState> CalendarRequest;
        public event Action<YearMonthMaybeDay> DateChanged;

        public bool Enabled {
            get => _yearInput.Enabled;
            set {
                if (value) {
                    _inputsContainer.RemoveAttribute(Magics.AttrDataReadOnly);
                } else {
                    _inputsContainer.SetValuelessAttribute(Magics.AttrDataReadOnly);    
                }
                
                _yearInput.Enabled = value;
                _monthInput.Enabled = value;
                _dayInput.Enabled = value;
                _hourInput.Enabled = value;
                _minuteInput.Enabled = value;
                _secondInput.Enabled = value;
            }}
        public HTMLElement Widget => _inputsContainer;
        public ISet<string> Errors => DefaultInputLogic.GetErrors(_inputsContainer);
        public bool IsValidating {
            get => _inputsContainer.ClassList.Contains(Magics.CssClassIsValidating);
            set => _inputsContainer.AddOrRemoveClass(value, Magics.CssClassIsValidating);
        }
        public ISet<string> DisabledReasons { set => DefaultInputLogic.SetDisabledReasons(_inputsContainer, value); }
        public DateTime? Value {
            get => _value;
            set {
                _value = value;
                OnInitValue();
            }}
        private readonly List<MaskedNumberInputView> _inputs = new List<MaskedNumberInputView>();
        private CalendarState? _calendarState;
        private int? Year => _yearInput.Value;
        private int? Month => _monthInput.Value;
        private int? Day => _dayInput.Value;
        private int? Hour => _hourInput.Value;
        private int? Minute => _minuteInput.Value;
        private int? Second => _secondInput.Value;
        
        public DateTimeInputView(
                DateTimeFormat precision, IEnumerable<Tuple<string,DateTimeElement?>> format, 
                DateTime? defaultValue,
                Tuple<DateTime?,DateTime?> validRange,
                IDateTimeBuilder dateTimeBuilder) {
            
            _precision = precision;
            _inputsContainer = new HTMLDivElement {ClassName = GetType().FullNameWithoutGenerics()};
            
            _yearInput = new MaskedNumberInputView(4, Tuple.Create(
                    validRange.Item1?.Year ?? 0001, 
                    validRange.Item2?.Year ?? 2999), 
                ConsumeDigitKey, '0');
            _monthInput = new MaskedNumberInputView(2, Tuple.Create(1, 12), ConsumeDigitKey, '0');
            _dayInput = new MaskedNumberInputView(2, Tuple.Create(1, 31), ConsumeDigitKey, '0'); 
            _hourInput = new MaskedNumberInputView(2, Tuple.Create(0, 23), ConsumeDigitKey, '0'); 
            _minuteInput = new MaskedNumberInputView(2, Tuple.Create(0, 59), ConsumeDigitKey, '0'); 
            _secondInput  = new MaskedNumberInputView(2, Tuple.Create(0, 59), ConsumeDigitKey, '0'); 
            
            format.ForEach(x => {
                if (x.Item1 != null) {
                    var fixedText = new HTMLDivElement { TextContent = x.Item1};
                    fixedText.AddClasses(Magics.CssClassDateTimeFormatFixedText);
                    _inputsContainer.AppendChild(fixedText);
                    return;
                }

                if (!x.Item2.HasValue) {
                    throw new Exception("expected either DateTimeElement or string");
                }

                MaskedNumberInputView curInput;

                switch (x.Item2.Value) {
                    case DateTimeElement.Year:   curInput = _yearInput; break;
                    case DateTimeElement.Month:  curInput = _monthInput; break;
                    case DateTimeElement.Day:    curInput = _dayInput; break;
                    case DateTimeElement.Hour:   curInput = _hourInput; break;
                    case DateTimeElement.Minute: curInput = _minuteInput; break;
                    case DateTimeElement.Second: curInput = _secondInput; break;
                    default: throw new Exception("unsupported DateTimeElement"); 
                }
                _inputsContainer.AppendChild(curInput.Widget);
                curInput.CalendarRequest += (y,forced) => {
                    CalendarState newState;

                    if (forced) {
                        _calendarState = y;
                        newState = y;
                    } else if (_calendarState.HasValue) {
                        newState = _calendarState.Value;
                    } else {
                        newState = y;
                    }

                    switch (newState) {
                        case CalendarState.Show:
                            CalendarRequest?.Invoke(CalendarState.Show);
                            break;

                        case CalendarState.Hide:
                            CalendarRequest?.Invoke(CalendarState.Hide);
                            break;

                        default: throw new Exception("unsupported CalendarState");
                    }
                };

                if (_inputs.Count <= 0) {
                    curInput.InputWidget.SetAttribute(Magics.AttrDataOptOutOfWholeTextSelectionOnFocus, "true");
                }
                _inputs.Add(curInput);
            });
            
            _validationOutcome = new HTMLDivElement();
            _validationOutcome.ClassList.Add(Magics.CssClassValidationState);
            _inputsContainer.AppendChild(_validationOutcome);

            _activateCalendar = InputTypeButtonActionView.CreateFontAwesomeIconedAction(
                IconFontType.FontAwesomeRegular, FontAwesomeRegular.IconCalendarAlt);
            _activateCalendar.Triggered += () => {
                if (!Enabled) {
                    return;
                }
                CalendarRequest?.Invoke(CalendarState.Show);
            };
            
            if (precision != DateTimeFormat.Y) {
                _inputsContainer.AppendChild(_activateCalendar.Widget);
            }

            _clearContent = InputTypeButtonActionView.CreateFontAwesomeIconedAction(
                IconFontType.FontAwesomeSolid, FontAwesomeSolid.IconTimes, Magics.CssClassClearContainer);
            _clearContent.Triggered += () => {
                if (!Enabled) {
                    return;
                }
                _value = null;
                OnInitValue();
                Changed?.Invoke(_value, true);
            };
            _inputsContainer.AppendChild(_clearContent.Widget);
            
            _yearInput.OnChange += () => OnInput(DateTimeElement.Year);
            _monthInput.OnChange += () => OnInput(DateTimeElement.Month);
            _dayInput.OnChange += () => OnInput(DateTimeElement.Day);
            _hourInput.OnChange += () => OnInput(DateTimeElement.Hour);
            _minuteInput.OnChange += () => OnInput(DateTimeElement.Minute);
            _secondInput.OnChange += () => OnInput(DateTimeElement.Second);
            
            _inputs.ForEach(x => {
                x.RequestNavigation += (y,preventDefault) => {
                    if (!y.Navigate.HasValue) {
                        throw new Exception("RequestNavigation with empty ProcessingOutcome.Navigate");
                    }

                    var i = _inputs.IndexOf(x);

                    Logger.Debug(GetType(), "RequestNavigation for {0}th element in dir={1} fon {2} elements", 
                        i, y.Navigate, _inputs.Count);

                    switch (y.Navigate.Value) {
                        case Direction.Forward:
                            if (i >= _inputs.Count-1) {
                                CalendarRequest?.Invoke(CalendarState.Hide);
                                return;
                            }
                            
                            if (!y.NavigateIsProgrammatic) {
                                preventDefault();
                            }
                            _inputs[i+1].FocusAtBeginning();
                            break;

                        case Direction.Backward:
                            if (i <= 0) {
                                CalendarRequest?.Invoke(CalendarState.Hide);
                                return;
                            }
                            
                            if (!y.NavigateIsProgrammatic) {
                                preventDefault();
                            }
                            _inputs[i-1].FocusAtEnding(y.PendingCharRemoval);
                            break;

                        default: throw new Exception("unsupported Direction");
                    }
                };
            });
            
            _value = defaultValue;
            _dateTimeBuilder = dateTimeBuilder;
            OnInitValue();
        }
        
        private char? ConsumeDigitKey(int keyCode) {
            if (keyCode >= Magics.KeyCodeNumpadZero && keyCode <= Magics.KeyCodeNumpadNine) {
                return (char)('0'+(keyCode-Magics.KeyCodeNumpadZero));
            }
            if (keyCode >= Magics.KeyCodeZero && keyCode <= Magics.KeyCodeNine) {
                return (char)('0'+(keyCode-Magics.KeyCodeZero));
            }
            return null;
        }

        private void OnInput(DateTimeElement? timeElement = null) {
            Logger.Debug(GetType(), "OnInput() _value={0}", _value);

            if (Month.HasValue && Year.HasValue && Day.HasValue) {
                var lastValidDay = DateTimeExtensions.BuildNextMonth(new DateTime(Year.Value, Month.Value, 1)).AddDays(-1).Day;

                if (Day > lastValidDay) {
                    _dayInput.Value = lastValidDay;
                }
            }

            if (timeElement.HasValue) {
                //calendar popup should already show choosen month (or even whole date)
                switch (timeElement.Value) {
                    case DateTimeElement.Year:
                    case DateTimeElement.Month:
                    case DateTimeElement.Day:
                        if (Month.HasValue && Year.HasValue && Day.HasValue) {
                            //initialize time fields
                            _value = _dateTimeBuilder.Build(Year.Value, Month.Value, Day.Value);
                            OnInitValue(); 
                        }

                        if (Month.HasValue && Year.HasValue) {
                            DateChanged?.Invoke(new YearMonthMaybeDay {
                                Year = Year.Value,
                                Month = Month.Value,
                                Day = Day
                            });
                        }
                        break;
                }
            }
            
            switch (_precision) {
                case DateTimeFormat.Y:
                    if (Year.HasValue) {
                        _value = _dateTimeBuilder.Build(Year.Value);
                        Changed?.Invoke(_value, true);
                    }
                    break;

                case DateTimeFormat.YM:
                    if (Year.HasValue && Month.HasValue) {
                        _value = _dateTimeBuilder.Build(Year.Value, Month.Value);
                        Changed?.Invoke(_value, true);
                    }
                    break;
                    
                case DateTimeFormat.DateOnly:
                    if (Year.HasValue && Month.HasValue && Day.HasValue) {
                        _value = _dateTimeBuilder.Build(Year.Value, Month.Value, Day.Value);
                        Changed?.Invoke(_value, true);
                    }
                    break;

                case DateTimeFormat.YMDhm:
                    if (Year.HasValue && Month.HasValue && Day.HasValue && Hour.HasValue && Minute.HasValue) {
                        _value = _dateTimeBuilder.Build(
                            Year.Value, Month.Value, Day.Value, Hour.Value, Minute.Value);
                        Changed?.Invoke(_value, true);
                    }
                    break;

                case DateTimeFormat.YMDhms:
                    if (Year.HasValue && Month.HasValue && Day.HasValue && Hour.HasValue && Minute.HasValue && Second.HasValue) {
                        _value = _dateTimeBuilder.Build(
                            Year.Value, Month.Value, Day.Value, 
                            Hour.Value, Minute.Value, Second.Value);
                        Changed?.Invoke(_value, true);
                    }
                    break;

                default: throw new Exception("unsupported DateTimeFormat");
            }
        }
        
        private void OnInitValue() {
            _yearInput.Value = _value?.Year;
            _monthInput.Value = _value?.Month;
            _dayInput.Value = _value?.Day;
            _hourInput.Value = _value?.Hour;
            _minuteInput.Value = _value?.Minute;
            _secondInput.Value = _value?.Second;
            
            Logger.Debug(GetType(), "OnInitValue() changed value to {0} - informing listeners", _value);
        }

        public void SetErrors(ISet<string> errors, bool causedByUser) {
            DefaultInputLogic.SetErrors(_inputsContainer, _inputsContainer, causedByUser, errors);
        }

        public void FocusBeginning() {
            _inputs.FirstOrDefault().FocusAtBeginning();
        }
    }
}
