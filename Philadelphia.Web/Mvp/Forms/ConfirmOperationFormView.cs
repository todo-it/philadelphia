using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class ConfirmOperationFormView : IFormView<HTMLElement> {
        public LabellessReadOnlyView Message { get; }
        public InputTypeButtonActionView Confirm { get; }
        
        public ConfirmOperationFormView(TextType inputType = TextType.TreatAsText) {
            Message = new LabellessReadOnlyView("div", inputType);
            Confirm = new InputTypeButtonActionView(I18n.Translate("Confirm"))
                .With(x => x.MarkAsFormsDefaultButton());
        }

        public RenderElem<HTMLElement>[] Render(HTMLElement _) {
            return new RenderElem<HTMLElement>[] {"<div>", Message, "</div>"};
        }

        public IView<HTMLElement>[] Actions => new IView<HTMLElement>[] {Confirm};
    }
}
