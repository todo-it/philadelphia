using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Web;

namespace Philadelphia.Demo.Client {
    public class AllFieldsFilledDataEntryFormView : IFormView<HTMLElement> {
        public InputTypeButtonActionView ConfirmAction = new InputTypeButtonActionView("Confirm")
            .With(x => x.MarkAsFormsDefaultButton());
        public IView<HTMLElement>[] Actions => new IView<HTMLElement>[]{ ConfirmAction };
        public InputView StringEntry = new InputView("Text entry");
        public InputView IntEntry = new InputView("Whole number entry");
        public InputView DecimalEntry = new InputView("Decimal entry (two fractional digits)");
        public LabellessReadOnlyView SummaryLine = new LabellessReadOnlyView();

        public RenderElem<HTMLElement>[] Render(HTMLElement parentContainer) {
            return new RenderElem<HTMLElement>[] {
                $"<div style='{Magics.CssClassTableLike}'>",
                StringEntry,
                IntEntry,
                DecimalEntry,
                SummaryLine,
                "</div><br>",
                @"<div class='grayedOut'>Notice that: <ul class='grayedOut' style='margin-top: 0;'>
                    <li>when form opens then for a brief while tooltips are shown</li>
                    <li>each control indicates its validation status</li>
                    <li>hovering mouse over input field reveals tooltip</li>
                    <li>button stays disabled until validation becomes positive</li>
                    <li>pressing enter within field causes 'default form button' to be activated</li>
                </ul></div>"
            };
        }
    }
}
