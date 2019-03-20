using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class RemoteActionsCallerFormView : IFormView<HTMLElement> {
        public IView<HTMLElement>[] Actions { get; } = {};
        public LabellessReadOnlyView Percentage = new LabellessReadOnlyView("span").WithCssClass("remoteActionsCallerPercentage");
        public LabellessReadOnlyView ErrorContainer = new LabellessReadOnlyView("span").WithCssClass("remoteActionsCallerError");

        public RenderElem<HTMLElement>[] Render(HTMLElement parent) {
            parent.ClassList.Add("remoteActionsParentContainer");
            
            return new RenderElem<HTMLElement>[] {
                "<div class='remoteActionsDirectContainer'>",
                $"<img src='{Magics.IconUrlSpinnerBig}'>",
                Percentage,
                "</div>",
                ErrorContainer};
        }
    }
}
