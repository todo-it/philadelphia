using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class RemoteValueChangeByEntryFormView<ViewT> : IFormView<HTMLElement> {
        public LabellessReadOnlyView Message { get; }
        public IReadWriteValueView<HTMLElement,ViewT> Input { get; }
        public InputTypeButtonActionView Confirm { get; }
        
        public RemoteValueChangeByEntryFormView(
            IReadWriteValueView<HTMLElement,ViewT> input, TextType inputType = TextType.TreatAsText) {

            Input = input;
            Message = new LabellessReadOnlyView("div", inputType);
            Confirm = new InputTypeButtonActionView(I18n.Translate("OK"))
                .With(x => x.MarkAsFormsDefaultButton());
        }

        public RenderElem<HTMLElement>[] Render(HTMLElement _) {
            return new RenderElem<HTMLElement>[] {
                "<div>",
                    Message,
                    $"<div class='{Magics.CssClassTableLike}'>",Input.Widget,"</div>",
                "</div>" };
        }

        public IView<HTMLElement>[] Actions => new IView<HTMLElement>[] {Confirm};
    }
}
