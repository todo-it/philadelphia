using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridge;
using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Demo.SharedModel;
using Philadelphia.Web;

namespace Philadelphia.Demo.Client {
    public enum MenuItems {
        DataInput,
        QrCodeScanner,
        PhotoTaker
    }

    public static class MenuItemsExtensions {
        public static string GetLabel(this MenuItems self) {
            switch (self) {
                case MenuItems.DataInput: return "Input and dialog";
                case MenuItems.QrCodeScanner: return "QR scanner";
                case MenuItems.PhotoTaker: return "Photo take";
                default: throw new Exception("unsupported MenuItem");
            }
        }
    }

    public class VerticalMenuFormView : IFormView<HTMLElement> {
        public IView<HTMLElement>[] Actions => ActionsBuilder.For();

        private readonly Func<Action, string, IActionView<HTMLElement>> _actionBuilder;
        private readonly HTMLDivElement _menuItemsContainer = new HTMLDivElement()
            .With(x => x.SetAttribute("style", "display: grid; grid-template-columns: auto; row-gap: 15px; padding: 10px"));
        
        public IEnumerable<Tuple<Action, string>> MenuItems { set {
            _menuItemsContainer.AppendAllChildren(
                value.Select(
                    actAndLbl => _actionBuilder(actAndLbl.Item1, actAndLbl.Item2).Widget));
        }}

        public VerticalMenuFormView(
                Func<Action, string, IActionView<HTMLElement>> actionBuilder) {
            
            _actionBuilder = actionBuilder;
        }

        public RenderElem<HTMLElement>[] Render(HTMLElement _) => 
            new RenderElem<HTMLElement>[] {_menuItemsContainer};
    }
    
    public class VerticalMenuForm<T> : IForm<HTMLElement,VerticalMenuForm<T>,CompletedOrCanceled> {
        private readonly string _title;
        private readonly bool _isCancellable;
        private readonly VerticalMenuFormView _view;
        public string Title => _title;
        public IFormView<HTMLElement> View => _view;
        public event Action<VerticalMenuForm<T>, CompletedOrCanceled> Ended;
        public T Chosen { get; private set; }
        public ExternalEventsHandlers ExternalEventsHandlers => 
            _isCancellable 
            ? ExternalEventsHandlers.Create(() => Ended?.Invoke(this, CompletedOrCanceled.Canceled))
            : ExternalEventsHandlers.Ignore;

        public VerticalMenuForm(
                string title, 
                IEnumerable<T> menuItems,
                Func<T,string> menuItemLabel,
                Func<string,IActionView<HTMLElement>> actionBuilder,
                bool isCancellable=true) {
            
            _title = title;
            _isCancellable = isCancellable;

            _view = new VerticalMenuFormView(
                (act, lbl) => {
                     var res = actionBuilder(lbl);
                     res.Triggered += () => act.Invoke();
                     return res;
                 });

            _view.MenuItems = menuItems.Select(x => {
                var xCpy = x;
                return Tuple.Create<Action, string>(() => {
                    Chosen = xCpy;
                    Ended?.Invoke(this, CompletedOrCanceled.Completed);
                }, menuItemLabel(x));
            });
        }
    }

    public class Program {
        private readonly PhillyContainer _di;
        private readonly FormRenderer _renderer;

        public Program(EnvironmentType env) {
            _di = new PhillyContainer();
            
            Services.Register(_di); //registers discovered services from model
            _di.RegisterAlias<IHttpRequester, BridgeHttpRequester>(LifeStyle.Singleton);
            _di.Register<MainMenuFlow>(LifeStyle.Transient);

            Toolkit.InitializeToolkit(null, x => _di.Resolve<ISomeService>().DataGridToSpreadsheet(x));

            var forcedCulture = DocumentUtil.GetQueryParameterOrNull("forcedCulture");
            I18n.ConfigureImplementation(() => new ToStringLocalization(forcedCulture));

            _renderer = Toolkit.DefaultFormRenderer();
        }

        private void OnReadyDesktop() {
            Document.Title = "Philadelphia Toolkit Demo App";
            
            new IntroFlow(Document.URL.Contains("skipWelcome")).Run(
                _renderer, 
                //when IntroFlow ends start MainMenuFlow
                //in this simplistic demo it is impossible to quit MainMenuFlow
                () => _di.Resolve<MainMenuFlow>().Run(_renderer) 
            );
        }

        private void OnReadyIAWApp() {
            var chooseActivity = new VerticalMenuForm<MenuItems>(
                "Demo choice",
                EnumExtensions.GetEnumValues<MenuItems>(),
                x => x.GetLabel(),
                lbl => new InputTypeButtonActionView(lbl),
                isCancellable:false);
            
            var inputName = new TextInputForm("Name", "What's your nickname?");
            
            var greetUser = new InformationalMessageForm("", "Greetings!");
            var scanResultView = new InformationalMessageFormView(customActionBuilder:x => new AnchorBasedActionView(x));
            var scanResult = new InformationalMessageForm(scanResultView, "", "Scan result");
            
            //quick hack for sake of demo
            var photoTakenResultView = new InformationalMessageFormView(customActionBuilder:x => new AnchorBasedActionView(x));
            photoTakenResultView.Message.Widget.RemoveAllChildren();
            var photoTakenResult = new InformationalMessageForm(photoTakenResultView,"","Your photo");
                
            var scanCodeView = new QrScannerFormView();
            var performScan = new QrScannerForm<string>(
                scanCodeView, "Scan QR code", "Any QR is fine", x => Task.FromResult(x));
            
            var takePhotoView = new PhotoTakerFormView();
            var takePhoto = new PhotoTakerForm(takePhotoView);
            
            chooseActivity.Ended += (x, outcome) => {
                _renderer.ClearMaster();
                switch (outcome) {
                    case CompletedOrCanceled.Completed:
                        switch (x.Chosen) {
                            case MenuItems.DataInput:
                                _renderer.ReplaceMaster(inputName);        
                                break;
                            
                            case MenuItems.QrCodeScanner:
                                _renderer.ReplaceMaster(performScan);
                                break;
                            
                            case MenuItems.PhotoTaker:
                                takePhoto.ClearImage();
                                _renderer.ReplaceMaster(takePhoto);
                                break;
                            
                            default: throw new Exception("unsupported MenuItems");
                        }
                        break;
                }
            };
            
            inputName.Ended += async (x, outcome) => {
                switch (outcome) {
                    case CompletedOrCanceled.Completed:
                        await greetUser.Init("Hi "+inputName.Introduced+"!");
                        _renderer.ReplaceMaster(greetUser);
                        break;
                    case CompletedOrCanceled.Canceled:
                        _renderer.ReplaceMaster(chooseActivity);
                        break;
                }
            };
            
            greetUser.Ended += (x, _) => _renderer.ReplaceMaster(inputName);

            performScan.Ended += async (x, outcome) => {
                switch (outcome) {
                    case CompletedOrCanceled.Completed:
                        await scanResult.Init("QR content: " + performScan.ScannedCode);
                        _renderer.AddPopup(scanResult);
                        break;
                    
                    case CompletedOrCanceled.Canceled:
                        _renderer.ReplaceMaster(chooseActivity);
                        break;
                }
                
            };
            scanResult.Ended += (x, _) => {
                _renderer.Remove(x);
                _renderer.ReplaceMaster(chooseActivity);
            };

            takePhoto.Ended += (x, outcome) => {
                switch (outcome) {
                    case CompletedOrCanceled.Completed:
                        var img = new HTMLImageElement();
                        img.Style.MaxWidth = "50vw";
                        img.Style.MaxHeight = "50vh";
                        
                        var fr = new FileReader();
                        fr.OnLoad += _ => img.Src = (string)fr.Result;
                        fr.ReadAsDataURL(takePhoto.PhotoAsFile);

                        photoTakenResultView.Message.Widget.ReplaceChildren(new[] {img});
                        _renderer.AddPopup(photoTakenResult);
                        break;
                    
                    case CompletedOrCanceled.Canceled:
                        _renderer.ReplaceMaster(chooseActivity);
                        break;
                }
            };
            photoTakenResult.Ended += (x, _) => {
                _renderer.Remove(x);
                _renderer.ReplaceMaster(chooseActivity);
            }; 
                
            _renderer.ReplaceMaster(chooseActivity);
        }
        
        [Ready]
        public static void OnReady() {
            var env = EnvironmentTypeUtil.GetInstanceFromWindow(Window.Instance);
            var pr = new Program(env);
            
            if (env == EnvironmentType.IndustrialAndroidWebApp) {
                pr.OnReadyIAWApp();
            } else {
                pr.OnReadyDesktop();
            }
        }
    }
}
