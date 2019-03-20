using System;
using Bridge;
using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Web;

namespace Navigation.Client {
    class MainFormView : IFormView<HTMLElement> {
        public InputTypeButtonActionView ShowInfo {get; } 
            = new InputTypeButtonActionView("Info popup");
        public InputTypeButtonActionView ReplaceMaster {get; } 
            = new InputTypeButtonActionView("Replace master");
        public IView<HTMLElement>[] Actions => ActionsBuilder.For(ShowInfo,ReplaceMaster); //shorter than explict array

        public RenderElem<HTMLElement>[] Render(HTMLElement parentContainer) {
            return new RenderElem<HTMLElement>[] {"this is main form body<br>using <i>some</i> html tags"};
        }
    }

    class MainForm : IForm<HTMLElement,MainForm,MainForm.Outcome> {
        public string Title => "Main form";
        private readonly MainFormView _view = new MainFormView();
        public IFormView<HTMLElement> View => _view;
        public event Action<MainForm, Outcome> Ended;

        public enum Outcome {
            EndRequested,
            InfoRequested,
            ReplaceMaster
        }
        
        public ExternalEventsHandlers ExternalEventsHandlers => ExternalEventsHandlers.Create(
            () => Ended?.Invoke(this, Outcome.EndRequested));//makes form cancelable
        
        public MainForm() {
            LocalActionBuilder.Build(_view.ShowInfo, () => Ended?.Invoke(this, Outcome.InfoRequested));
            LocalActionBuilder.Build(_view.ReplaceMaster, () => Ended?.Invoke(this, Outcome.ReplaceMaster));
        }
    }

    class AltMainFormView : IFormView<HTMLElement> {
        public IView<HTMLElement>[] Actions => new IView<HTMLElement>[0];
        public RenderElem<HTMLElement>[] Render(HTMLElement parentContainer) {
            return new RenderElem<HTMLElement>[] {@"that's an alternative main form.<br>
                It has no actions so it's a dead end (press F5 to restart)"};
        }
    }

    class AltMainForm : IForm<HTMLElement,AltMainForm,Unit> {
        public string Title => "Main form";
        public IFormView<HTMLElement> View { get; } = new AltMainFormView();
        public event Action<AltMainForm, Unit> Ended;
        public ExternalEventsHandlers ExternalEventsHandlers => ExternalEventsHandlers.Ignore;
    }

    public static class NavigationFlow {
        public static void Run(IFormRenderer<HTMLElement> renderer) {
            var mainFrm = new MainForm();
            var showInfo = new InformationalMessageForm("Some important info", "Info form title");
            var altMainFrm = new AltMainForm();
            renderer.ReplaceMaster(mainFrm);

            mainFrm.Ended += (form, outcome) => {
                switch (outcome) {
                    case MainForm.Outcome.EndRequested:
                        renderer.Remove(form);
                        break;

                    case MainForm.Outcome.InfoRequested:
                        renderer.AddPopup(showInfo);
                        break;

                    case MainForm.Outcome.ReplaceMaster:
                        renderer.ReplaceMaster(altMainFrm);
                        break;
                }
            };
            
            showInfo.Ended += (form, _) => renderer.Remove(form); //just dismiss this popup (no relevant outcome)
        }
    }

    public class Program {
        [Ready]
        public static void OnReady() {
            Toolkit.InitializeToolkit();
            var renderer = Toolkit.DefaultFormRenderer(LayoutModeType.TitleExtra_Actions_Body);
            NavigationFlow.Run(renderer);
        }
    }
}
