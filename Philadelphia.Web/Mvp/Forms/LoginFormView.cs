using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class LoginFormView : IFormView<HTMLElement> {
        public InputView Login { get; } = new InputView(I18n.Translate("Username"))
            .With(x => x.InputWidget.Name = "username"); //to make autocomplete work better
        public InputView Password { get; } = new InputView(I18n.Translate("Password"), InputView.TypePassword)
            .With(x => x.InputWidget.Name = "password"); //to make autocomplete work better
        public InputTypeButtonActionView AttemptLogin { get; } 
            = new InputTypeButtonActionView(I18n.Translate("Login"))
                .With(x => x.MarkAsFormsDefaultButton());
        
        public RenderElem<HTMLElement>[] Render(HTMLElement _) {
            return new RenderElem<HTMLElement>[] {
                $"<div class='{Magics.CssClassTableLike}' style='position: relative'>",
                Login,
                Password,
                "</div>"
            };
        }

        public IView<HTMLElement>[] Actions => new IView<HTMLElement>[] {AttemptLogin};
    }
}