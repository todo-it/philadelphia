using System;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class LoginFormView : IFormView<HTMLElement> {
        public InputView Login { get; } = new InputView(I18n.Translate("Username"))
            .With(x => x.InputWidget.Name = "username")
            .With(x => x.InputWidget.SetAttribute("autocapitalize", "none")); //to make autocomplete work better
        public InputView Password { get; } = new InputView(I18n.Translate("Password"), InputView.TypePassword)
            .With(x => x.InputWidget.Name = "password"); //to make autocomplete work better
        public IActionView<HTMLElement> AttemptLogin { get; }

        public LoginFormView(Func<LabelDescr,IActionView<HTMLElement>> customActionBuilder = null) {
            AttemptLogin = (customActionBuilder ?? Toolkit.DefaultActionBuilder)
                .Invoke(new LabelDescr {Label = I18n.Translate("Login")})
                .With(x => x.MarkAsFormsDefaultButton());
        }

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
