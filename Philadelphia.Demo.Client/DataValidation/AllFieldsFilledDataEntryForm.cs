using System;
using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Web;

namespace Philadelphia.Demo.Client {
    public class AllFieldsFilledDataEntryForm : IBareForm<HTMLElement> {
        public readonly Observable.IObservable<Unit> Ended;
        public string Title => "Data validation demo";
        public IFormView<HTMLElement> View { get; }

        public AllFieldsFilledDataEntryForm() {
            var view = new AllFieldsFilledDataEntryFormView();
            var ended = new Observable.Publisher<Unit>();
            
            Ended = ended;
            View = view;

            var strEntry = LocalValueFieldBuilder.Build(
                view.StringEntry,
                (x, errors) => errors.IfTrueAdd(
                    string.IsNullOrWhiteSpace(x) || x.Length < 4,
                    "Text must be at least 4 chars long"),
                (x, errors) => errors.IfTrueAdd(
                    string.IsNullOrWhiteSpace(x) || x.Length > 10,
                    "Text must be no longer than 10 chars long")
            );

            var intEntry = LocalValueFieldBuilder.BuildNullableInt(
                view.IntEntry, 
                (x, errors) => errors.IfTrueAdd(!x.HasValue || x < 1234, "Number must be bigger than 1234")
            );

            var decEntry = LocalValueFieldBuilder.BuildNullableDecimal(
                view.DecimalEntry, 
                DecimalFormat.WithTwoDecPlaces,
                (x, errors) => errors.IfTrueAdd(!x.HasValue || x < 5.6m, 
                    "Number must be bigger than "+I18n.Localize(5.6m, DecimalFormat.WithOneDecPlace))
            );

            //this field is used to show that values on screen are not necessarily values within model.
            //In other words: when validator rejected value from user wrong value and error message stays on screen BUT model keeps its last-accepted-value
            var summaryLine = LocalValueFieldBuilder.Build("", view.SummaryLine);

            void UpdateSummaryLine() {
                var decVal = !decEntry.Value.HasValue ? "" : I18n.Localize(decEntry.Value.Value, DecimalFormat.WithOneDecPlace);

                summaryLine.DoProgrammaticChange(
                    $@"Accepted vals: Text={strEntry.Value} Int={intEntry.Value} Decimal={decVal}"
                );
            }

            strEntry.Changed += (_, __, ___, ____, _____) => UpdateSummaryLine();
            intEntry.Changed += (_, __, ___, ____, _____) => UpdateSummaryLine();
            decEntry.Changed += (_, __, ___, ____, _____) => UpdateSummaryLine();

            UpdateSummaryLine();
            
            var confirm = LocalActionBuilder.Build(view.ConfirmAction, () => ended.Fire(Unit.Instance));
            confirm.BindEnableAndInitializeAsObserving(x => {
                x.Observes(strEntry); 
                x.Observes(intEntry); 
                x.Observes(decEntry); 
            });
        }

        //form won't be cancelable
        public ExternalEventsHandlers ExternalEventsHandlers => ExternalEventsHandlers.Ignore;
    }
}
