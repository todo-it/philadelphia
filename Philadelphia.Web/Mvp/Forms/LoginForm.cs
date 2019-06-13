using System;
using System.Linq;
using System.Threading.Tasks;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class LoginForm : IForm<HTMLElement,LoginForm,CompletedOrCanceled> {
        public string LoggedUserName { get; private set; }
        public event Action<LoginForm,CompletedOrCanceled> Ended;
        public Func<string> TitleProvider {get; set;}
        public string Title => 
            TitleProvider != null ? TitleProvider?.Invoke() : I18n.Translate("Logging to system");

        public IFormView<HTMLElement> View { get; }
        public string ErrorMessageOrNull { get; private set;}

        public LoginForm(Func<string,string,Task<string>> service, Action<string> storeCsrfToken) : 
            this(new LoginFormView(), service, storeCsrfToken) {}

        public LoginForm(LoginFormView view, Func<string,string,Task<string>> service, Action<string> storeCsrfToken) {
            View = view;

            var login = new LocalValue<string>("", "");
            login.AddValidatorAndRevalidate(
                (x, errors) => errors.IfTrueAdd(string.IsNullOrWhiteSpace(x), I18n.Translate("Field cannot be empty"))
            );
            view.Login.BindReadWriteAndInitialize(login);


            var passwd = new LocalValue<string>("", "");
            passwd.AddValidatorAndRevalidate(
                (x, errors) => errors.IfTrueAdd(string.IsNullOrWhiteSpace(x), I18n.Translate("Field cannot be empty"))
            );
            view.Password.BindReadWriteAndInitialize(passwd);
            
            var mayAttemptLogin = new AggregatedErrorsValue<bool>(false, self => !self.Errors.Any(), x => {
                x.Observes(login); 
                x.Observes(passwd); });
            
            var attemptLogin = RemoteActionBuilder.Build(view.AttemptLogin, 
                () => service(login.Value, passwd.Value),
                x => {
                    storeCsrfToken(x);
                    LoggedUserName = login.Value;
                    ErrorMessageOrNull = null;
                    Ended?.Invoke(this, CompletedOrCanceled.Completed);
                },
                x => {
                    LoggedUserName = null;
                    ErrorMessageOrNull = x.ErrorMessage;
                    Ended?.Invoke(this, CompletedOrCanceled.Canceled);
                });
            attemptLogin.BindEnableAndInitialize(mayAttemptLogin);
        }

        public ExternalEventsHandlers ExternalEventsHandlers => ExternalEventsHandlers.Ignore;
    }
}
