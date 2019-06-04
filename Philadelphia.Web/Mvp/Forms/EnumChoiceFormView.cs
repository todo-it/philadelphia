using System;
using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Web;

namespace Philadelphia.Web {
    public class EnumChoiceFormView<T> : IFormView<HTMLElement> where T:struct {
        public IView<HTMLElement>[] Actions => ActionsBuilder.For(Confirm);
        public RadioBasedSingleChoice Choice { get; }
        public LabellessReadOnlyView Description { get; }
            = new LabellessReadOnlyView();
        public InputTypeButtonActionView Confirm { get; }
            = new InputTypeButtonActionView("OK").With(x => x.MarkAsFormsDefaultButton());

        public EnumChoiceFormView(
                T defaultValue, Func<T,string> getLabel, 
                Action<EnumChoiceFormView<T>> postInitialization = null) {

            Choice = RadioBasedSingleChoiceUtilExtensions.BuildForEnum(defaultValue, getLabel);
            postInitialization?.Invoke(this);
        }

        public RenderElem<HTMLElement>[] Render(HTMLElement parentContainer) {
            parentContainer.ClassName = GetType().FullNameWithoutGenerics();
            return new RenderElem<HTMLElement>[] {Description, Choice};
        }
    }
}
