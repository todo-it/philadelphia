using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class AuthenticationThenMainMenuFlow<UserT> : IFlow<HTMLElement> {
        private readonly Func<UserT,Func<IFormRenderer<HTMLElement>>,IEnumerable<MenuItemUserModel>> _menuItemsProvider;
        private readonly RemoteActionsCallerForm _fetchUser;
        private readonly LoginForm<UserT> _loginForm;
        private readonly MenuForm _mainMenuForm;
        private readonly HorizontalLinksMenuFormView _mainMenuFormView;
        private readonly InformationalMessageForm _authProblemMsg;
        private readonly RemoteActionsCallerForm _runLogout;
        private readonly ConfirmMessageForm _logoutConfirm;
        private IFormRenderer<HTMLElement> _baseRenderer;
        private UserT _currentUserOrNull;

        public AuthenticationThenMainMenuFlow(
                Func<Task<Tuple<string,UserT>>> fetchCsrfAndUserOrNull, 
                Func<string,string,Task<Tuple<string, UserT>>> loginByUserAndPasswdGettingCsrfAndUser,
                Func<Task<Unit>> logoutOper,
                Func<UserT,Func<IFormRenderer<HTMLElement>>,IEnumerable<MenuItemUserModel>> menuItemsProvider,
                Func<MenuItemModel,Tuple<HTMLElement,Action<string>>> customItemBuilder = null,
                Func<string> programNameProvider = null,
                Func<LabelDescr,IActionView<HTMLElement>> customActionBuilder = null) {

            _menuItemsProvider = menuItemsProvider;
            
            _fetchUser = new RemoteActionsCallerForm(new RemoteActionsCallerFormView(), x => 
                x.Add(fetchCsrfAndUserOrNull, y => {
                    StoreCsrf(y?.Item1);
                    _currentUserOrNull = y != null ? y.Item2 : default(UserT);
                }));

            var loginFormView = new LoginFormView(customActionBuilder);
            _loginForm = new LoginForm<UserT>(loginFormView, loginByUserAndPasswdGettingCsrfAndUser, (csrfToken, curUser) => {
                StoreCsrf(csrfToken);
                _currentUserOrNull = curUser;
            });
            if (programNameProvider != null) {
                _loginForm.TitleProvider = () => programNameProvider();
            }

            _mainMenuFormView = new HorizontalLinksMenuFormView(customItemBuilder);
            _mainMenuForm = new MenuForm(_mainMenuFormView, new List<MenuItemUserModel>());
            _authProblemMsg = new InformationalMessageForm("", I18n.Translate("Authentication problem"));
            _runLogout = new RemoteActionsCallerForm(x => x.Add(logoutOper));
            _logoutConfirm = new ConfirmMessageForm(
                I18n.Translate("Are you sure you want to log out?"), I18n.Translate("Logging out"));
        }

        private void StoreCsrf(string token) => Toolkit.CsrfToken = token;

        public void Run(IFormRenderer<HTMLElement> renderer, Action atExit) {
            _baseRenderer = renderer;
            renderer.AddPopup(_fetchUser);

            _mainMenuForm.Ended += (x, _) => renderer.AddPopup(_logoutConfirm);

            _logoutConfirm.Ended += (x, outcome) => {
                switch (outcome) {
                    case CompletedOrCanceled.Completed:
                        renderer.Remove(x);
                        renderer.AddPopup(_runLogout);
                        break;

                    case CompletedOrCanceled.Canceled:
                        renderer.Remove(x);
                        break;
                        
                    default: throw new Exception("unsupported outcome");
                }
            };

            _fetchUser.Ended += (x, outcome) => {
                switch (outcome) {
                    case RemoteActionsCallerForm.Outcome.Succeeded:
                        renderer.Remove(x);

                        if (_currentUserOrNull == null) {
                            renderer.AddPopup(_loginForm);
                            break;
                        }

                        //user is logged in -> continue
                        PopulateMenuItems();
                        renderer.ReplaceMaster(_mainMenuForm);
                        atExit();
                        break;

                    case RemoteActionsCallerForm.Outcome.Interrupted:
                    case RemoteActionsCallerForm.Outcome.Canceled:
                        break;

                    default: throw new Exception("unsupported outcome");
                }
            };

            _loginForm.Ended += async (x, outcome) => {
                switch (outcome) {
                    case CompletedOrCanceled.Completed:
                        renderer.Remove(x);

                        //user is logged in -> continue
                        PopulateMenuItems();
                        renderer.ReplaceMaster(_mainMenuForm);
                        atExit();
                        break;

                    case CompletedOrCanceled.Canceled:
                        await _authProblemMsg.Init(
                            _loginForm.ErrorMessageOrNull);
                        renderer.AddPopup(_authProblemMsg);
                        break;

                    default: throw new Exception("unsupported outcome");
                }
            };
            _runLogout.Ended += (x,outcome) => {
                switch (outcome) {
                    case RemoteActionsCallerForm.Outcome.Canceled:
                    case RemoteActionsCallerForm.Outcome.Interrupted:
                        renderer.Remove(x);
                        break;

                    case RemoteActionsCallerForm.Outcome.Succeeded:
                        renderer.Remove(x);
                        renderer.Remove(_mainMenuForm);
                        renderer.AddPopup(_loginForm);
                        break;
                            
                    default: throw new Exception("unsupported outcome");
                }
            };

            _authProblemMsg.Ended += (x, unit) => renderer.Remove(x);
        }

        private void PopulateMenuItems() {
            _mainMenuForm.Menu.ReplaceItems(_menuItemsProvider(_currentUserOrNull, CreateRenderer));
        }

        public IFormRenderer<HTMLElement> CreateRenderer() {
            return _baseRenderer.CreateRendererWithBase(
                new ElementWrapperFormCanvas(
                    Toolkit.BaseFormCanvasTitleStrategy, 
                    _mainMenuFormView.BodyPanel.Widget, 
                    Toolkit.DefaultExitButtonBuilder, 
                    Toolkit.DefaultLayoutMode));
        }
    }
}
