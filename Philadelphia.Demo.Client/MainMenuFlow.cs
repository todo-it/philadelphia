using System;
using System.Collections.Generic;
using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Demo.SharedModel;
using Philadelphia.Web;
using static Philadelphia.Common.MenuItemUserModel;

namespace Philadelphia.Demo.Client {
    public class MainMenuFlow : IFlow<HTMLElement> {
        private readonly MenuForm _mainMenuForm;
        private readonly HorizontalLinksMenuFormView _mainMenuFormView;
        private IFormRenderer<HTMLElement> _baseRenderer;
        private readonly InformationalMessageForm _aboutMsg, _licensesInfoMsg;
        private IFormRenderer<HTMLElement> _lastRenderer;

        public MainMenuFlow(ISomeService someService, ITranslationsService translationsService, IHttpRequester httpRequester) {
            IFormRenderer<HTMLElement> CreateRenderer() =>
                _baseRenderer.CreateRendererWithBase(
                    new ElementWrapperFormCanvas(
                        Toolkit.BaseFormCanvasTitleStrategy,
                        _mainMenuFormView.BodyPanel.Widget,
                        Toolkit.DefaultExitButtonBuilder, 
                        Toolkit.DefaultLayoutMode));

            _aboutMsg = new InformationalMessageForm(
                new InformationalMessageFormView(TextType.TreatAsHtml),
                "<b>Philadelphia Toolkit Demo</b><br>by TODO IT spółka z o.o.",
                "About program");
            _aboutMsg.Ended += (x, _) => _lastRenderer.Remove(x);

            _licensesInfoMsg = new InformationalMessageForm(
                new InformationalMessageFormView(TextType.TreatAsHtml),
                OpenSourceLicensesText.OpenSourceLicensesHtml,
                I18n.Translate("Used open source licensed programs and libraries"));
            _licensesInfoMsg.Ended += (x, _) => _lastRenderer.Remove(x);

            var menuItems = new List<MenuItemUserModel> {
                CreateSubTree("Features",
                    CreateLocalLeaf(
                        "Server-sent events", 
                        () => new SseDemoFlow(someService).Run(CreateRenderer())),
                    CreateLocalLeaf(
                        "Forms navigation", 
                        () => new NavigationProgram().Run(CreateRenderer())),
                    CreateLocalLeaf(
                        "Internationalization", 
                        () => new InternationalizationFlow(translationsService).Run(CreateRenderer()))),
                CreateSubTree("Data validation",
                    CreateLocalLeaf(
                        "Simplest", 
                        () => new ValidationProgram().Run(CreateRenderer())),
                    CreateLocalLeaf(
                        "Tabbed view indicator", 
                        () => new TabbedViewValidationFlow().Run(CreateRenderer())),
                    CreateLocalLeaf(
                        "File uploads", 
                        () => new UploaderDemoFlow(someService, httpRequester).Run(CreateRenderer())) ),
                CreateSubTree("Widgets",
                    CreateLocalLeaf(
                        "Databound datagrid", 
                        () => new DataboundDatagridProgram(someService).Run(CreateRenderer())),
                    CreateLocalLeaf(
                        "Datetime pickers", 
                        () => new DateTimeDemoProgram().Run(CreateRenderer())),
                    CreateLocalLeaf(
                        "Dropdowns", 
                        () => new DropdownsProgram().Run(CreateRenderer())),
                    CreateLocalLeaf(
                        "Master details", 
                        () => new MasterDetailsProgram(someService).Run(CreateRenderer())),
                    CreateLocalLeaf(
                        "Flexible layout", 
                        () => new FlexibleLayoutFlow().Run(CreateRenderer())) ),
                CreateSubTree("Help",
                    CreateLocalLeaf(
                        "About program", 
                        () => {
                            _lastRenderer = CreateRenderer();
                            _lastRenderer.AddPopup(_aboutMsg);
                        }),
                    CreateLocalLeaf(
                        "Open source licenses", 
                        () => {
                            _lastRenderer = CreateRenderer();
                            _lastRenderer.AddPopup(_licensesInfoMsg);
                        })
                    )
            };
            
            //TODO dropdown with not-legal-anymore/scratched value
            //TODO add I18n demo
            
            _mainMenuFormView = new HorizontalLinksMenuFormView();
            _mainMenuForm = new MenuForm(_mainMenuFormView, menuItems);
        }

        public void Run(IFormRenderer<HTMLElement> renderer, Action atExit) {
            renderer.ReplaceMaster(_mainMenuForm);
            _baseRenderer = renderer;
        }
    }
}
