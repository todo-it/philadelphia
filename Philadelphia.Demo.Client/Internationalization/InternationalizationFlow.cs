using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Demo.SharedModel;
using Philadelphia.Web;

namespace Philadelphia.Demo.Client {
    public class InternationalizationFlow : IFlow<HTMLElement> {
        private readonly EnumChoiceForm<SupportedLang> _choose;
        private readonly RemoteActionsCallerForm _downloadTranslation;
        private readonly InformationalMessageForm _welcomeDialog;
        
        public InternationalizationFlow(ITranslationsService service) {
            _downloadTranslation = new RemoteActionsCallerForm(x => 
                x.Add(
                    () => _choose.ChosenValue, 
                    service.FetchTranslation, 
                    y => I18n.ConfigureImplementation(
                        () => new TranslationWithFallbackI18n(_choose.ChosenValue.ToString(), y))));

            _choose = new EnumChoiceForm<SupportedLang>(
                "Language choice", 
                true, 
                SupportedLang.EN, 
                x => x.GetLangName(),
                x => {
                    x.Choice.Widget.Style.Display = Display.Grid;
                    x.Choice.Widget.Style.GridTemplateColumns = "auto 1fr";

                    x.Description.Widget.InnerHTML = 
                        @"For sake of simplicity you need to make explicit choice below. 
In a normal program, you would take current language either from logged in
user property or from browser's Accept-Language header field.
If you study source code you will see that messages eligable for translation are declared as: 
    I18n.Translate(""Some message that should be localized"")
Those messages can be easily found and translated within JSON file using <a target='_blank' href='https://github.com/d-p-y/oldschool-i18n'>OldSchool-I18n</a>";

                    x.Description.Widget.Style.WhiteSpace = WhiteSpace.Pre;
                    x.Description.Widget.Style.PaddingBottom = "20px";
                    x.Description.Widget.ClassName = "grayedOut";
                });

            _welcomeDialog = new InformationalMessageForm();
        }

        private async Task InitWelcomeDialog() {
            await _welcomeDialog.Init(
                I18n.Translate("Welcome"),
                I18n.Translate("Localized greeting"));
        }

        public void Run(IFormRenderer<HTMLElement> renderer, Action atExit) {
            renderer.AddPopup(_choose);

            _choose.Ended += async (x, outcome) => {
                switch (outcome) {
                    case CompletedOrCanceled.Canceled:
                        renderer.Remove(x);
                        atExit();
                        break;

                    case CompletedOrCanceled.Completed:
                        renderer.Remove(x);
                        if (_choose.ChosenValue == SupportedLang.EN) {
                            await InitWelcomeDialog();
                            renderer.AddPopup(_welcomeDialog); //go directly to dialog
                        } else {
                            renderer.AddPopup(_downloadTranslation);
                        }
                        
                        break;

                    default: throw new Exception("unsupported outcome");
                }
            };

            _downloadTranslation.Ended += async (x, outcome) => {
                switch (outcome) {
                    case RemoteActionsCallerForm.Outcome.Canceled:
                    case RemoteActionsCallerForm.Outcome.Interrupted:
                        renderer.Remove(x);
                        break;

                    case RemoteActionsCallerForm.Outcome.Succeeded:
                        renderer.Remove(x);
                        await InitWelcomeDialog();
                        renderer.AddPopup(_welcomeDialog);
                        break;
                    
                    default: throw new Exception("unsupported outcome");
                }
            };

            _welcomeDialog.Ended += (x, unit) => renderer.Remove(x);
        }
    } 
}
