using System;
using System.Collections.Generic;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class DateTimePickerView : IReadWriteValueView<HTMLElement,DateTime?> {        
        private readonly LocalValue<DateTime> _curMonth; //which month is visible in popup
        private readonly DateTimeFormat _precision;
        private DateTime? _value; //really choosen date
        private readonly Action<HTMLElement> _extraDayBuilderActionOrNull;
        private readonly HTMLElement _container;
        private readonly DateTimeInputView _inputsContainer;
        private readonly HTMLDivElement _calendar;
        private readonly HTMLSpanElement _calendarContainer;
        private readonly HTMLDivElement _yearAndMonthChoice;

        // REVIEW: inline local only fields
        private readonly InputTypeButtonActionView _minusYear,_minusMonth,_plusMonth,_plusYear;
        private readonly LabellessReadOnlyView _yearAndMthLbl;
        private readonly HTMLElement _lbl;
        private DateTime? _curCalendarMonth;
        private readonly Tuple<DateTime?,DateTime?> _allowedRange;
        private readonly IDateTimeBuilder _dateTimeBuilder;

        public HTMLElement Widget => _container;
        public DateTimePickerMode Mode { get; }
        public LocalValue<DateTime?> OtherDateTime {get; }
        public HTMLElement Label => _lbl;
        public DateTimeInputView InputsContainer => _inputsContainer;
        public HTMLElement Calendar => _calendar;
        public event ValueChangedSimple<DateTime?> Changed;
        public event UiErrorsUpdated ErrorsChanged;
        public DateTime? Value { get {return _value;}
            set {
                OnChanged(value, false);
                _inputsContainer.Value = value;
            }
        }
        public ISet<string> Errors => _inputsContainer.Errors;
        public bool IsValidating {
            get => _inputsContainer.IsValidating;
            set => _inputsContainer.IsValidating = value;
        }
        public bool Enabled {
            get => _inputsContainer.Enabled;
            set => _inputsContainer.Enabled = value;
        }
        public ISet<string> DisabledReasons { set => _inputsContainer.DisabledReasons = value; }

        private DateTimePickerView(
                string label, string extraPopupLabelOrNull, 
                DateTimeFormat precision, 
                IEnumerable<Tuple<string,DateTimeElement?>> format, 
                DateTime? initialValue, DateTime? otherInitialValue, 
                Tuple<DateTime?,DateTime?> allowedRange, 
                DateTimePickerMode mode,
                IDateTimeBuilder dateTimeBuilder,
                Action<HTMLElement> extraDayBuilderActionOrNull = null,
                PopupLocation popupLocation = PopupLocation.Right) {
                
            _dateTimeBuilder = dateTimeBuilder;
            _precision = precision;
            var id = UniqueIdGenerator.GenerateAsString();
            OtherDateTime = new LocalValue<DateTime?>(otherInitialValue);
            OtherDateTime.Changed += (sender, oldValue, newValue, errors, isUserChange) => {
                Logger.Debug(GetType(), "repopulatingcalendar of mode={0} due to othervalue changed to={1}/{2}",
                    mode, newValue, OtherDateTime.Value);
                RepopulateCalendar();
            };
            Mode = mode;

            _allowedRange = allowedRange;
            var v = initialValue ?? _dateTimeBuilder.BuildFrom(DateTime.Now, _precision).BuildMonth();
            _curMonth = new LocalValue<DateTime>(v);
            
            _value = initialValue;

            switch (mode) {
                case DateTimePickerMode.From:
                case DateTimePickerMode.To:
                    _extraDayBuilderActionOrNull = extraDayBuilderActionOrNull;
                    break;

                case DateTimePickerMode.Sole:
                    _extraDayBuilderActionOrNull = x => x.SetAttribute(Magics.AttrDataForId, id);
                    break;

                default: throw new Exception("unsupported DateTimePickerMode");
            }
            
            _lbl = new HTMLLabelElement {
                TextContent = label,
                HtmlFor = id };

            _inputsContainer = new DateTimeInputView(
                precision, format, _value, allowedRange, _dateTimeBuilder);

            _inputsContainer.DateChanged += async ymd => {
                if (ymd.Day.HasValue) {
                    var newDate = 
                        _inputsContainer.Value 
                            ?? _dateTimeBuilder.Build(ymd.Year, ymd.Month, ymd.Day.Value);
                    _value = newDate;
                    Logger.Debug(GetType(), "input DateChanged without day value to {0} - forcing calendar view rebuild", _value);

                    await _curMonth.DoChange(newDate, true, this, false);
                    RepopulateCalendar();

                    //TODO should we add time set strategy here?
                    Changed?.Invoke(_value, true);
                } else {
                    
                    Logger.Debug(GetType(), "input DateChanged with day value to {0} - forcing calendar view rebuild", _value);

                    await _curMonth.DoChange(
                        _dateTimeBuilder.Build(
                            ymd.Year, ymd.Month, 1 /*irrelevant*/), true, this, false);
                    RepopulateCalendar();
                }
            };
            _inputsContainer.Changed += (newValue, isUserInput) => {
                _value = newValue;

                Logger.Debug(GetType(), "input Changed value to {0} - forcing calendar view rebuild", _value);

                RepopulateCalendar();
                if (isUserInput) {
                    if (!newValue.HasValue) {
                        //on clear let user retype it
                        _inputsContainer.FocusBeginning();
                    }

                    Changed?.Invoke(newValue, isUserInput);
                }
            };

            if (_value.HasValue) {
                OnChanged(_value.Value, false);
            }

            _calendarContainer = new HTMLSpanElement();
            _calendarContainer.Style.Position = Position.Relative;
            
            _yearAndMonthChoice = new HTMLDivElement();
            _yearAndMonthChoice.AddClasses(Magics.CssClassYearAndMonthChoice);
            
            _minusYear = InputTypeButtonActionView.CreateFontAwesomeIconedAction(
                IconFontType.FontAwesomeSolid, FontAwesomeSolid.IconBackward);
            _minusMonth = InputTypeButtonActionView.CreateFontAwesomeIconedAction(
                IconFontType.FontAwesomeSolid, FontAwesomeSolid.IconChevronLeft);
            _plusMonth = InputTypeButtonActionView.CreateFontAwesomeIconedAction(
                IconFontType.FontAwesomeSolid, FontAwesomeSolid.IconChevronRight);
            _plusYear = InputTypeButtonActionView.CreateFontAwesomeIconedAction(
                IconFontType.FontAwesomeSolid, FontAwesomeSolid.IconForward);

            _yearAndMthLbl = new LabellessReadOnlyView();
            _yearAndMthLbl.BindReadOnlyAndInitialize(_curMonth, x => $"{x.Year} {I18n.GetFullMonthName(x.Month)}");
            _yearAndMthLbl.Widget.AddClasses(Magics.CssClassYearAndMonthName);
            _yearAndMthLbl.Widget.AddOrRemoveClass(_curMonth.Value.IsCurrentMonth(), Magics.CssClassCurrent);

            if (extraPopupLabelOrNull != null) {
                _yearAndMonthChoice.AppendChild(new HTMLDivElement { TextContent = extraPopupLabelOrNull});
            }
            _yearAndMonthChoice.AppendChild(_minusYear.Widget);
            _yearAndMonthChoice.AppendChild(_minusMonth.Widget);
            _yearAndMonthChoice.AppendChild(_yearAndMthLbl.Widget);
            _yearAndMonthChoice.AppendChild(_plusMonth.Widget);
            _yearAndMonthChoice.AppendChild(_plusYear.Widget);

            _calendar = new HTMLDivElement();
            _calendar.AddClasses(mode.AsCssClassName(), precision.AsCssClassName());
            
            if (precision == DateTimeFormat.YM) {
                //month chooser
                _calendar.OnMouseOut += ev => {
                    _calendar.RemoveClasses(Magics.CssClassActiveFormerMonth);
                    _calendar.RemoveClasses(Magics.CssClassActiveThisMonth);
                    _calendar.RemoveClasses(Magics.CssClassActiveNextMonth);
                };

                _calendar.OnMouseOver += ev => {
                    if (!ev.HasHtmlTarget()) {
                        return;
                    }

                    var el = ev.HtmlTarget();
                    if (el.GetElementOrItsAncestorMatchingOrNull(x => x.ClassList.Contains(Magics.CssClassFormerMonthDay)) != null) {
                        _calendar.AddClasses(Magics.CssClassActiveFormerMonth);
                    } else if (el.GetElementOrItsAncestorMatchingOrNull(x => x.ClassList.Contains(Magics.CssClassThisMonthDay)) != null) {
                        _calendar.AddClasses(Magics.CssClassActiveThisMonth);
                    } else if (el.GetElementOrItsAncestorMatchingOrNull(x => x.ClassList.Contains(Magics.CssClassNextMonthDay)) != null) {
                        _calendar.AddClasses(Magics.CssClassActiveNextMonth);
                    }
                };
            }

            RepopulateCalendar();
            
            _yearAndMthLbl.Widget.OnClick += async ev => {
                ev.PreventDefault();
                await _curMonth.DoChange(
                    _dateTimeBuilder.BuildFrom(DateTime.Now, _precision), true, this);
            };
            _minusYear.Triggered += async () => {
                await _curMonth.DoChange(
                    _dateTimeBuilder.BuildFrom(_curMonth.Value.AddYears(-1), _precision), 
                    true, this);
            };
            _minusMonth.Triggered += async () =>  {
                await _curMonth.DoChange(
                    _dateTimeBuilder.BuildFrom(_curMonth.Value.AddMonths(-1), _precision), 
                    true, this);
            };
            _plusMonth.Triggered += async () =>  {
                await _curMonth.DoChange(
                    _dateTimeBuilder.BuildFrom(_curMonth.Value.AddMonths(1), _precision), 
                    true, this);
            };
            _plusYear.Triggered += async () =>  {
                await _curMonth.DoChange(
                    _dateTimeBuilder.BuildFrom(_curMonth.Value.AddYears(1), _precision), 
                    true, this);
            };

            _curMonth.Changed += (x, oldValue, newValue, errors, iUserChange) => {
                RepopulateCalendar(); //even if another day in the same month (to switch 'current' item)
                
                _yearAndMthLbl.Widget.AddOrRemoveClass(_curMonth.Value.IsCurrentMonth(), Magics.CssClassCurrent);
            };

            if (mode == DateTimePickerMode.Sole) {
                _container = new HTMLDivElement();
                _container.AddClasses(typeof(DateTimePickerView).FullNameWithoutGenerics());

                _container.AppendChild(_lbl);

                var inputAndPopupCntnr = new HTMLDivElement();
                inputAndPopupCntnr.AddClasses(popupLocation.AsCssClass());
                inputAndPopupCntnr.AppendChild(_inputsContainer.Widget);
                
                _calendarContainer.AddClasses(Magics.CssClassPopupContainer);
                
                inputAndPopupCntnr.AppendChild(_calendarContainer);
                
                _container.AppendChild(inputAndPopupCntnr);
                
                _inputsContainer.CalendarRequest += x => {
                    switch (x) {
                        case CalendarState.Show:
                            ShowPopupIfNeeded();
                            break;

                        case CalendarState.Hide:
                            HidePopupIfNeeded();
                            break;

                        default: throw new Exception("unsupported CalendarState");
                    }
                };
                
                DocumentUtil.AddMouseClickListener(_container, ev => {
                    if (!ev.HasHtmlTarget()) {
                        return;
                    }

                    if (ev.HtmlTarget().GetElementOrItsAncestorMatchingOrNull(x => 
                            x == _container || id.Equals(x.GetAttribute(Magics.AttrDataForId))) == null) {
                        
                        HidePopupIfNeeded();
                    }
                });
            }
        }

        public void HidePopupIfNeeded() {
            if (!_calendarContainer.HasChildNodes()) {
                //already hidden
                return;
            }
            _calendarContainer.RemoveChild(_calendar);
        }
        
        public void ShowPopupIfNeeded() {
            if (_precision == DateTimeFormat.Y) {
                return;
            }

            if (_calendarContainer.HasChildNodes()) {
                //already shown
                return;
            }

            _calendarContainer.AppendChild(_calendar);
        }

        public void SetErrors(ISet<string> errors, bool causedByUser) {
            _inputsContainer.SetErrors(errors, causedByUser);
            ErrorsChanged?.Invoke(this, errors);
        }

        private void OnChanged(DateTime? newValue, bool originatingFromUser) {
            _value = newValue;
            _inputsContainer.Value = newValue;
        }

        private void RepopulateCalendar() {
            var reallyBuild = 
                !_curCalendarMonth.HasValue || 
                _curCalendarMonth.Value.Year != _curMonth.Value.Year ||
                _curCalendarMonth.Value.Month != _curMonth.Value.Month;
            _curCalendarMonth = _curMonth.Value;
            var today = _dateTimeBuilder.BuildFrom(DateTime.Now, DateTimeFormat.DateOnly);
            var thisMonth = _curMonth.Value.Month;
            var formerMonth = DateTimeExtensions.BuildFormerMonth(_curMonth.Value).Month;
            var nextMonth = DateTimeExtensions.BuildNextMonth(_curMonth.Value).Month;
            var theoretLastDay = DateTimeExtensions.BuildNextMonth(_curMonth.Value);
            var childIdx = 0;
            
            Logger.Debug(GetType(), "RepopulateCalendar reallyBuild?={0} _curCalendarMonth={1} _curMonth={2}", 
                reallyBuild, _curCalendarMonth, _curMonth);

            if (reallyBuild) {
                _calendar.RemoveAllChildren();
                _calendar.AppendChild(_yearAndMonthChoice);
                _calendar.AddClasses(Magics.CssClassDaysOfMonth);
                
                //week day labels
                var dowLbl = I18n.GetFirstDayOfWeek();
                for (var i=0; i<7; i++) {
                    _calendar.AppendChild(new HTMLDivElement {TextContent =  I18n.GetWeekDayAcronym(dowLbl)});
                    dowLbl = dowLbl.GetNextDay();
                }
            } else {
                childIdx += 1+7; //_yearAndMonthChoice + 7 days labels
            }
            
            var dow = I18n.GetFirstDayOfWeek();

            //find out former month trailing days
            var iDay = DateTimeExtensions.BuildThisMonth(_curMonth.Value);
            while (iDay.DayOfWeek != dow) {
                iDay = iDay.AddDays(-1);
            }

            //add days for:
            //-former month trailing days (if any)
            //-current month days
            //-next month trailing days (if any)
            while (
                iDay < theoretLastDay || 
                iDay.Month == nextMonth && iDay.DayOfWeek != I18n.GetFirstDayOfWeek() ) { //padded week
                
                var day = reallyBuild ? 
                    new HTMLAnchorElement {
                        TextContent = iDay.Day.ToString(),
                        Href = "#"} 
                    : 
                    _calendar.GetChildAtOrNull(childIdx);

                _extraDayBuilderActionOrNull?.Invoke(day);
                day.RemoveAllCssClasses();
                day.AddClasses(IsInRange(iDay) ? Magics.CssClassValidDay : Magics.CssClassInvalidDay);

                if (iDay.Month == formerMonth) {
                    day.AddClasses(Magics.CssClassFormerMonthDay);
                } else if (iDay.Month == thisMonth) {
                    day.AddClasses(Magics.CssClassThisMonthDay);
                } else if (iDay.Month == nextMonth) {
                    day.AddClasses(Magics.CssClassNextMonthDay);
                }
                    
                if (iDay.Equals(today)) {
                    day.AddClasses(Magics.CssClassToday);
                }

                //only emphasise choosen dates if they are really choosen
                
                switch(Mode) {
                    case DateTimePickerMode.Sole:
                        switch (_precision) {
                            case DateTimeFormat.DateOnly:
                            case DateTimeFormat.YMDhm:
                            case DateTimeFormat.YMDhms:
                                if (_value.HasValue && iDay.Date.Equals(_value.Value.Date)) {
                                    day.AddClasses(Magics.CssClassChoosen);
                                }
                                break;

                            case DateTimeFormat.YM:
                                if (!_value.HasValue || !iDay.IsSameMonthAs(_value.Value)) {
                                    break;
                                }

                                if (iDay.Day == 1) {
                                    day.AddClasses(Magics.CssClassSince);
                                } else if (iDay.Day == iDay.BuildLastDayOfMonth()) {
                                    day.AddClasses(Magics.CssClassUntil);
                                } else {
                                    day.AddClasses(Magics.CssClassInRange);
                                }
                                break;

                            case DateTimeFormat.Y: break; //unreachable code
                            default: throw new Exception("unsupported DateTimeFormat");
                        }
                            
                        break;

                    case DateTimePickerMode.From:
                        if (_value.HasValue && iDay.Date.Equals(_value.Value.Date)) {
                            day.AddClasses(Magics.CssClassSince);
                        } else if (OtherDateTime.Value.HasValue) {
                            var other = OtherDateTime.Value.Value;

                            if (other.Date.Equals(iDay.Date)) {
                                day.AddClasses(Magics.CssClassUntil);
                            } else if (other > iDay && _value.HasValue && iDay > _value.Value.Date) {
                                day.AddClasses(Magics.CssClassInRange);
                            }
                        }
                        break;

                    case DateTimePickerMode.To:
                        if (_value.HasValue && iDay.Date.Equals(_value.Value.Date)) {
                            day.AddClasses(Magics.CssClassUntil);
                        } else if (OtherDateTime.Value.HasValue) {
                            var other = OtherDateTime.Value.Value;

                            if (other.Date.Equals(iDay.Date)) {
                                day.AddClasses(Magics.CssClassSince);
                            } else if (other < iDay && _value.HasValue && iDay < _value.Value.Date) {
                                day.AddClasses(Magics.CssClassInRange);
                            }
                        }
                        break;
                }
            
                if (reallyBuild) {
                    var iDayCopy = iDay;
                    
                    day.OnClick += async ev => {
                        ev.PreventDefault(); //doesn't navigate to '#'
                        var isValid = IsInRange(iDayCopy);

                        Logger.Debug(GetType(), "day {0} is valid choice?={1}", 
                            iDayCopy, isValid);
                        
                        if (!isValid) {
                            return;
                        }

                        DateTime newValue;

                        switch (_precision) {
                            case DateTimeFormat.YM:
                                switch (Mode) {
                                    case DateTimePickerMode.Sole:
                                    case DateTimePickerMode.From:
                                        newValue = _dateTimeBuilder.Build(
                                            iDayCopy.Year, iDayCopy.Month);
                                        break;

                                    case DateTimePickerMode.To:
                                        newValue = _dateTimeBuilder.BuildFrom(
                                            DateTimeExtensions.BuildNextMonth(iDayCopy).AddDays(-1),
                                            _precision);
                                        break;

                                    default: throw new Exception("unsupported DateTimePickerMode");
                                }
                                break;
                                    
                            case DateTimeFormat.DateOnly:
                            case DateTimeFormat.YMDhm:
                            case DateTimeFormat.YMDhms:
                                newValue = _dateTimeBuilder.BuildFrom(iDayCopy, _precision);
                                break;

                            default:throw new Exception("unsupported DateTimeFormat");
                        }
                        
                        Logger.Debug(GetType(), "Mode {0} changing value to {1}", Mode, newValue);
                        OnChanged(newValue, true);
                        await _curMonth.DoChange(newValue, true, this, true);
                        Changed?.Invoke(newValue, true);
                    };
                    
                    _calendar.AppendChild(day);
                }
               
                iDay = iDay.AddDays(1);
                childIdx++;
            }
        }

        private bool IsInRange(DateTime inp) {
            switch (Mode) {
                case DateTimePickerMode.From:
                    if (OtherDateTime.Value.HasValue && inp.Date > OtherDateTime.Value.Value) {
                        return false;
                    }
                    break;

                case DateTimePickerMode.To:
                    if (OtherDateTime.Value.HasValue && inp.Date < OtherDateTime.Value.Value) {
                        return false;
                    }
                    break;

                case DateTimePickerMode.Sole:
                    break; //no extra check

                default: throw new Exception("unsupported DateTimePickerMode");
            }

            if (!_allowedRange.Item1.HasValue && !_allowedRange.Item2.HasValue) {
                return true;
            }

            if (_allowedRange.Item1.HasValue && inp < _allowedRange.Item1.Value) {
                return false;
            }

            if (_allowedRange.Item2.HasValue && inp > _allowedRange.Item2.Value) {
                return false;
            }

            return true;
        }

        public static DateTimePickerView CreateSoleEntry(
            string label, DateTimeFormat precision, 
            IEnumerable<Tuple<string,DateTimeElement?>> format, 
            DateTime? initialValue,
            Tuple<DateTime?,DateTime?> validRange,
            IDateTimeBuilder customDateTimeBuilder = null,
            PopupLocation popupLocation = PopupLocation.Right) {

            return new DateTimePickerView(
                label, null, precision, format, initialValue, null, validRange, 
                DateTimePickerMode.Sole, 
                customDateTimeBuilder ?? LocalDateTimeBuilder.InstanceBeginOfDay,
                null, popupLocation);
        }

        public static DateTimePickerView CreateRangeFromEntry(
            string entryLabel, string popupLabel, 
            DateTimeFormat precision, 
            IEnumerable<Tuple<string,DateTimeElement?>> format, 
            DateTime? initialValue, DateTime? otherInitialValue,
            Tuple<DateTime?,DateTime?> validRange,
            IDateTimeBuilder customDateTimeBuilder = null,
            Action<HTMLElement> extraDayBuilderAction = null) {
            
            return new DateTimePickerView(
                entryLabel, popupLabel, precision, format, initialValue, otherInitialValue, 
                validRange, DateTimePickerMode.From, 
                customDateTimeBuilder ?? LocalDateTimeBuilder.InstanceBeginOfDay,
                extraDayBuilderAction);
        }
        
        public static DateTimePickerView CreateRangeToEntry(
            string entryLabel, string popupLabel, 
            DateTimeFormat precision, IEnumerable<Tuple<string,DateTimeElement?>> format, 
            DateTime? initialValue, DateTime? otherInitialValue,
            Tuple<DateTime?,DateTime?> validRange,
            IDateTimeBuilder customDateTimeBuilder = null,
            Action<HTMLElement> extraDayBuilderAction = null) {
            
            return new DateTimePickerView(
                entryLabel, popupLabel, precision, format, initialValue, otherInitialValue, 
                validRange, DateTimePickerMode.To, 
                customDateTimeBuilder ?? LocalDateTimeBuilder.InstanceBeginOfDay,
                extraDayBuilderAction);
        }

        public static implicit operator RenderElem<HTMLElement>(DateTimePickerView inp) {
            return RenderElem<HTMLElement>.Create(inp);
        }
    }
}
