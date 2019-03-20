using System;
using System.Globalization;
using System.Linq;
using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Web;

namespace Philadelphia.Demo.Client {
    public class DateTimeDemoForm : IForm<HTMLElement,DateTimeDemoForm,Unit> {
        public event Action<DateTimeDemoForm, Unit> Ended;
        public string Title => "DateTime pickers demo";
        public IFormView<HTMLElement> View => _view;

        private readonly DateTimeDemoFormView _view;

        public DateTimeDemoForm() {
            _view = new DateTimeDemoFormView();

            LocalValueFieldBuilder.BuildDateTimePicker(_view.SingleValidation, null, Validator.IsNotNull, (newValue, errors) => {
                if (!newValue.HasValue) {
                    return;
                }

                errors.IfTrueAdd(newValue.Value.Day % 2 != 0, "Day must be even");
                errors.IfTrueAdd(newValue.Value.Hour % 2 != 0, "Hour must be even"); });
            
            LocalValueFieldBuilder.Build(
                Tuple.Create<DateTime?,DateTime?>(null,null), _view.RangeValidation, 
                (newValue, errors) => {
                    if (!newValue.Item1.HasValue || !newValue.Item2.HasValue) {
                        errors.Add("Range not choosen");
                        return;
                    }

                    if (!(newValue.Item2.Value - newValue.Item1.Value).TotalDays.AreApproximatellyTheSame(2, 0.1)) {
                        errors.Add("Exactly three days need");
                        Logger.Debug(GetType(), "Exactly three days need but selected {0} to {1} days={2}", 
                            newValue.Item1.Value, newValue.Item2.Value,
                            (newValue.Item2.Value - newValue.Item1.Value).TotalDays);
                        return;
                    } 

                    for (var iDay=newValue.Item1.Value; iDay <=newValue.Item2.Value; iDay = iDay.AddDays(1)) {
                        if (iDay.DayOfWeek == DayOfWeek.Saturday || iDay.DayOfWeek == DayOfWeek.Sunday) {
                            errors.Add("Weekends are not permitted");
                            return;
                        }
                    } });
        }

        public ExternalEventsHandlers ExternalEventsHandlers =>
            ExternalEventsHandlers.Create(() => Ended?.Invoke(this, Unit.Instance));
    }
}
