using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class TextInputFormView : IFormView<HTMLElement> {
        public LabellessReadOnlyView Label { get; }
        public InputView Input { get; } = new InputView();
        public InputTypeButtonActionView Confirm { get; }
        
        public TextInputFormView(TextType inputType = TextType.TreatAsText) {
            Label = new LabellessReadOnlyView("div", inputType);
            Confirm = new InputTypeButtonActionView(I18n.Translate("OK"))
                .With(x => x.MarkAsFormsDefaultButton());
        }

        public RenderElem<HTMLElement>[] Render(HTMLElement _) {
            return new RenderElem<HTMLElement>[] {
                "<div>",
                Label,
                $"<div class='{Magics.CssClassTableLike}'>",Input,"</div>",
                "</div>" };
        }

        public IView<HTMLElement>[] Actions => new IView<HTMLElement>[] {Confirm};
    }
}
