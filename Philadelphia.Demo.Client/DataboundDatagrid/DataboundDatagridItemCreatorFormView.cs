using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Demo.SharedModel;
using Philadelphia.Web;

namespace Philadelphia.Demo.Client {
    public class DataboundDatagridItemCreatorFormView : IFormView<HTMLElement> {
        public InputView SomeText { get; } = new InputView("SomeText");
        public InputView SomeNumber { get; } = new InputView("SomeNumber");
        public InputCheckboxView SomeBool { get; } = new InputCheckboxView("SomeBool");
        public SingleChoiceDropDown<SomeTraitType?> SomeTrait { get; } = CommonDropdowns.BuildSomeTraitType();

        public InputTypeButtonActionView CreateAction {get; } = 
            new InputTypeButtonActionView("Create").With(x => x.MarkAsFormsDefaultButton());
        public IView<HTMLElement>[] Actions => ActionsBuilder.For(CreateAction);

        public RenderElem<HTMLElement>[] Render(HTMLElement parentContainer) {
            return new RenderElem<HTMLElement>[] {
                $"<div class='{Magics.CssClassTableLike}'>",
                SomeText,
                SomeNumber,
                SomeBool,
                SomeTrait,
                "</div>"
            };
        }
    }
}
