using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Demo.SharedModel;
using Philadelphia.Web;

namespace Philadelphia.Demo.Client {
    public class IntroduceItemFormView : IFormView<HTMLElement> {
        public InputView SomeText { get; } = new InputView("SomeText");
        public InputView SomeNumber { get; } = new InputView("SomeNumber", InputView.TypeNumber);
        public InputCheckboxView SomeBool { get; } = new InputCheckboxView("SomeBool");
        public SingleChoiceDropDown<SomeTraitType?> SomeTrait { get; } = CommonDropdowns.BuildSomeTraitType();
        public InputTypeButtonActionView Create = new InputTypeButtonActionView("Save");
        public IView<HTMLElement>[] Actions => ActionsBuilder.For(Create);

        private readonly HorizontalTabbedView _tabbedView;

        public IntroduceItemFormView() {
            _tabbedView = HorizontalTabbedView
                .CreateTableLikeObserved(x => {
                        x.Add(SomeText);
                        x.Add(SomeNumber);
                        return "Basic";},
                    x => {
                        x.Add(SomeBool);
                        return "Additional";},
                    x => {
                        x.Add(SomeTrait);
                        return "Crucial";})
                .With(x => x.TabContentContainer.Style.MinWidth = "315px");
        }

        public RenderElem<HTMLElement>[] Render(HTMLElement parentContainer) {
            return new RenderElem<HTMLElement>[] {_tabbedView};
        }
    }
}
