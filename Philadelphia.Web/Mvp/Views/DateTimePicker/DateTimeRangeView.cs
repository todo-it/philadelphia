using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public enum PopupLocation {
        Right,
        Bottom
    }

    public static class PopupLocationExtensions {
        public static string AsCssClass(this PopupLocation self) {
            return $"PopupLocation_{self.ToString()}";
        }
    }

    public class DateTimeRangeView : IReadWriteValueView<HTMLElement,Tuple<DateTime?,DateTime?>> {
        private readonly DateTimeFormat _precision;
        private readonly HTMLElement _container = new HTMLDivElement();
        private readonly DateTimePickerView _since;
        private readonly DateTimePickerView _to;
        private readonly LocalValue<Tuple<DateTime?,DateTime?>> _value;
        private readonly HTMLDivElement _popupsCtn,_popups;
        
        public HTMLElement Widget => _container;
        public HTMLElement PopupContainer => _popupsCtn;
        public ISet<string> Errors => DefaultInputLogic.GetErrors(_container);
        public bool IsValidating {
            get => _container.ClassList.Contains(Magics.CssClassIsValidating);
            set => _container.AddOrRemoveClass(value, Magics.CssClassIsValidating);
        }
        public ISet<string> DisabledReasons { set => DefaultInputLogic.SetDisabledReasons(_container, value); }
        public bool Enabled {
            set {
                _since.Enabled = value;
                _to.Enabled = value;
            }}
        public Tuple<DateTime?, DateTime?> Value {
            get => _value.Value;
            set {
                ValueChange(value, false); //FIXME will not wait for completition :(
            }}
        public event UiErrorsUpdated ErrorsChanged;

        // REVIEW: valuetuple?
        public event ValueChangedSimple<Tuple<DateTime?, DateTime?>> Changed;

        public DateTimeRangeView(
                string label, string fromLabel, string tillLabel, 
                DateTimeFormat precision, 
                // REVIEW: valuetuple?
                IEnumerable<Tuple<string,DateTimeElement?>> formatRaw, 
                Tuple<DateTime?,DateTime?> initialValue, 
                Func<DateTime,DateTime> timeForDateSince,
                Func<DateTime,DateTime> timeForDateTill,
                Tuple<DateTime?,DateTime?> validRange,
                PopupLocation popupLocation) {

            _precision = precision;

            var format = formatRaw.ToList();

            _container.AddClasses(typeof(DateTimeRangeView).FullNameWithoutGenerics());
            var id = UniqueIdGenerator.GenerateAsString();
            _container.Id = id;

            switch (precision) {
                case DateTimeFormat.YM:
                    initialValue = Tuple.Create(
                        initialValue.Item1?.BuildMonth(),
                        initialValue.Item2?.BuildLastMomentOfMonth() );
                    break;
            }

            _value = new LocalValue<Tuple<DateTime?, DateTime?>>(initialValue);
            _value.Changed += (sender, oldValue, newValue, errors, isUserChange) => {
                if (!isUserChange) {
                    return;
                }
                Changed?.Invoke(_value.Value, isUserChange);
            };

            var lbl = new HTMLLabelElement { TextContent = label};
            _container.AppendChild(lbl);

            _since = DateTimePickerView.CreateRangeFromEntry(
                fromLabel, fromLabel, precision, format, initialValue.Item1, initialValue.Item2, 
                timeForDateSince,
                validRange,
                x => x.SetAttribute(Magics.AttrDataForId, id));
            
            _to = DateTimePickerView.CreateRangeToEntry(
                tillLabel, tillLabel, precision, format, initialValue.Item2, initialValue.Item1, 
                timeForDateTill,
                validRange,
                x => x.SetAttribute(Magics.AttrDataForId, id));

            _since.Changed += async (newValue, isUserInput) => {
                await _to.OtherDateTime.DoChange(newValue, false, this, false);
                if (!isUserInput) {
                    return;
                }
                await _value.DoChange(Tuple.Create(newValue, _since.OtherDateTime.Value), isUserInput, this);
                Logger.Debug(GetType(), "range newvalue due to 'Since' changed {0}->{1}", 
                    newValue, _since.OtherDateTime.Value);
            };
            _to.Changed += async (newValue, isUserInput) => {
                await _since.OtherDateTime.DoChange(newValue, false, this, false);
                if (!isUserInput) {
                    return;
                }
                await _value.DoChange(Tuple.Create(_to.OtherDateTime.Value, newValue), isUserInput, this);
                Logger.Debug(GetType(), "range newvalue due to 'Till' changed {0}->{1}", 
                    _to.OtherDateTime.Value, newValue);
            };

            _popupsCtn = new HTMLDivElement();
            _popupsCtn.AddClasses(Magics.CssClassPopupContainer);
            
            _popups = new HTMLDivElement();
            _popups.AddClasses(Magics.CssClassPopups);
            _popups.AppendChild(_since.Calendar);
            _popups.AppendChild(_to.Calendar);
            
            _since.InputsContainer.CalendarRequest += x => {
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
            
            _to.InputsContainer.CalendarRequest += x => {
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
                
                if (ev.HtmlTarget().GetElementOrItsAncestorMatchingOrNull(
                        x => _container == x || id.Equals(x.GetAttribute(Magics.AttrDataForId))) == null) {

                    HidePopupIfNeeded();
                }
            });

            var entriesCnt = new HTMLDivElement();
            entriesCnt.AppendChild(_since.Label);
            entriesCnt.AppendChild(_since.InputsContainer.Widget);
            
            if (popupLocation == PopupLocation.Right) {
                entriesCnt.AppendChild(_popupsCtn);
            }
            
            entriesCnt.AppendChild(_to.Label);
            entriesCnt.AppendChild(_to.InputsContainer.Widget);
            
            if (popupLocation == PopupLocation.Bottom) {
                entriesCnt.AppendChild(_popupsCtn);
            }

            _container.AddClasses(popupLocation.AsCssClass());
            _container.AppendChild(entriesCnt);
        }
        
        public void SetErrors(ISet<string> errors, bool causedByUser) {
            DefaultInputLogic.SetErrors(_container, _container, causedByUser, errors);
        }
        
        private async Task ValueChange(Tuple<DateTime?, DateTime?> value, bool isUserChange) {
            await _value.DoChange(value, false, this); 

            _since.Value = value.Item1;
            _to.Value = value.Item2;

            await _since.OtherDateTime.DoChange(value.Item2, isUserChange);
            await _to.OtherDateTime.DoChange(value.Item1, isUserChange);
        }

        private void ShowPopupIfNeeded() {
            if (_precision == DateTimeFormat.Y) {
                return;
            }

            if (_popupsCtn.HasChildNodes()) {
                //already shown
                return;
            }
            _popupsCtn.AppendChild(_popups);
        }

        private void HidePopupIfNeeded() {
            Logger.Debug(GetType(), "HidePopupIfNeeded()");
            if (!_popupsCtn.HasChildNodes()) {
                //already hidden
                return;
            }
            Logger.Debug(GetType(), "HidePopupIfNeeded() hiding");
            _popupsCtn.RemoveChild(_popups);
        }

        public static implicit operator RenderElem<HTMLElement>(DateTimeRangeView inp) {
            return RenderElem<HTMLElement>.Create(inp);
        }
    }
}
