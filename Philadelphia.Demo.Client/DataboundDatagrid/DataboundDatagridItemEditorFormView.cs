using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Demo.SharedModel;
using Philadelphia.Web;

namespace Philadelphia.Demo.Client {
    public class DataboundDatagridItemEditorFormView : IFormView<HTMLElement> {
        public IView<HTMLElement>[] Actions => new IView<HTMLElement>[0];
        public InputView SomeNumber { get; } = new InputView("SomeNumber").With(x => x.EnableClickToEdit());
        public InputView SomeText { get; } = new InputView("SomeText").With(x => x.EnableClickToEdit());
        public InputCheckboxView SomeBool { get; } = new InputCheckboxView("SomeBool").With(x => x.EnableClickToEdit());
        public SingleChoiceDropDown<SomeTraitType?> SomeChoice { get; } 
            = CommonDropdowns.BuildSomeTraitType().With(x => x.EnableClickToEdit());

        public RenderElem<HTMLElement>[] Render(HTMLElement parentContainer) {
            return new RenderElem<HTMLElement>[]{
                $"<div class='grayedOut'>Click on dashed content to reveal editor</div><br>" +
                $"<div class='{Magics.CssClassTableLike}'>",
                SomeNumber,
                SomeText,
                SomeBool,
                SomeChoice,
                "</div>" };
        }
    }
}
