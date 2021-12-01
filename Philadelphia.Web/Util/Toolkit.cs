using System;
using System.Threading.Tasks;
using Philadelphia.Common;
using Bridge.Html5;

namespace Philadelphia.Web {
    public static class Toolkit {
        public static bool ClickingOutsideOfDialogDismissesIt { get; set; } 
        public static LayoutModeType DefaultLayoutMode { get; set; } = LayoutModeType.TitleExtra_Body_Actions;
        public static Func<IHtmlFormCanvas,ITitleFormCanvasStrategy> BaseFormCanvasTitleStrategy  { get; set; }
        public static Func<IActionView<HTMLElement>> BaseFormCanvasExitButtonBuilderOrNull { get; set; } 
            
        public static Func<LabelDescr,IActionView<HTMLElement>> DefaultActionBuilder { get; set; } = 
            x => new InputTypeButtonActionView(x);
            
        public static void InitializeToolkit(
                ILoggerImplementation customLogger = null, 
                Func<DatagridContent,Task<FileModel>> spreadsheetBuilder = null,
                bool? clickingOutsideOfDialogDismissesIt = null) {

            ExecOnUiThread.SetImplementation(
                async x => {
                    x();
                    await Task.Run(() => {}); //NOP
                },
                async x => {
                    await x();
                });
            
            if (customLogger != null) {
                Logger.ConfigureImplementation(customLogger);
            } else if (DocumentUtil.HasQueryParameter("debug") && DocumentUtil.GetQueryParameter("debug") == "lite") {
                var toIgnoreByType = new [] {
                    typeof(Philadelphia.Web.GeneralAttrChangedObserver),
                    typeof(Philadelphia.Web.GeneralChildListChangedObserver),
                    typeof(Philadelphia.Web.SpecificChildListChangedObserver),
                    typeof(Philadelphia.Web.TooltipManager),
                    typeof(Philadelphia.Web.MouseObserver),
                    typeof(Philadelphia.Web.FormCanvasExtensions),
                    typeof(Philadelphia.Web.SpecificResizeObserver) };

                var toIgnoreByBaseName = new [] { 
                    typeof(Philadelphia.Web.DataGridColumn<string,Unit>).FullNameWithoutGenerics() };

                Logger.ConfigureImplementation(new ForwardMatchingToConsoleLogLoggerImplementation(
                    x => !toIgnoreByType.Contains(x) && !toIgnoreByBaseName.Contains(x.FullNameWithoutGenerics()) ));
            } else if (DocumentUtil.HasQueryParameter("debug")) {
                Logger.ConfigureImplementation(new ForwardEverythingToConsoleLogLoggerImplementation());
            } else {
                Logger.ConfigureImplementation(new ForwardErrorsToConsoleLogLoggerImplementation());
            }
            
            DocumentUtil.Initialize(); //initialize tooltips, esc handler etc
            
            var env = EnvironmentTypeUtil.GetInstanceFromWindow(Window.Instance);
            Document.Body.SetAttribute(Magics.AttrDataEnvironment, env.AsDataEnvironmentAttributeValue());

            Func<DatagridContent,Task<FileModel>> noExportImpl = 
                _ => {
                    throw new Exception("spreadsheet builder not provided via DataGridSettings.Init()");
                };

            DataGridSettings.Init(spreadsheetBuilder ?? noExportImpl);

            Document.OnDragEnter += ev => {
                Document.Body.SetAttribute(Magics.AttrDataDraggingFile, "x");
                ev.PreventDefault();
            };
            //detect real end of dragging (leaving outside of browser)
            //https://stackoverflow.com/questions/3144881/how-do-i-detect-a-html5-drag-event-entering-and-leaving-the-window-like-gmail-d#14248483
            Document.OnDragLeave += ev => {
                var clientX = (int)ev.GetFieldValue("clientX");
                var clientY = (int)ev.GetFieldValue("clientY");

                if (clientX <= 0 || clientY <= 0) {
                    Document.Body.RemoveAttribute(Magics.AttrDataDraggingFile);
                }

                Logger.Debug(typeof(Toolkit), "Document dragleave {0}", ev.Target);
            };
            
            switch (env) {
                case EnvironmentType.Desktop:
                    BaseFormCanvasTitleStrategy = x => new RegularDomElementTitleFormCanvasStrategy(x);
                    BaseFormCanvasExitButtonBuilderOrNull = DefaultExitButtonBuilder;
                    ClickingOutsideOfDialogDismissesIt = clickingOutsideOfDialogDismissesIt ?? false;
                    break;
                
                case EnvironmentType.IndustrialAndroidWebApp:
                    BaseFormCanvasTitleStrategy = x => new BodyBasedPropagatesToAppBatTitleFormCanvasStrategy(x);
                    BaseFormCanvasExitButtonBuilderOrNull = null;
                    ClickingOutsideOfDialogDismissesIt = clickingOutsideOfDialogDismissesIt ?? true;
                
                    IawAppApi.SetOnBackPressed(() => {
                        Logger.Debug(typeof(Toolkit), "backbutton pressed");
                        var consumed = DocumentUtil.TryCloseTopMostForm();
                        Logger.Debug(typeof(Toolkit), "backbutton consumed?={0}", consumed);
                        return consumed;
                    });
                    break;
                
                default: throw new Exception("unsupported environment");
            }
        }

        public static FormRenderer DefaultFormRenderer() =>
            new FormRenderer(
                new ElementWrapperFormCanvas(
                    BaseFormCanvasTitleStrategy, Document.Body, 
                    BaseFormCanvasExitButtonBuilderOrNull, DefaultLayoutMode),
                    new FactoryMethodProvider<IFormCanvas<HTMLElement>>(() => new ModalDialogFormCanvas(ClickingOutsideOfDialogDismissesIt)) );
        
        public static CalculateTbodyHeight DefaultTableBodyHeightProvider(int fixedHeightToAdd=0) {
             return (el,theaderHeight,_) => el.GetAvailableHeightForFormElement(0, 2) - theaderHeight + fixedHeightToAdd;
        }

        public static IActionView<HTMLElement> DefaultExitButtonBuilder() {
            var result = new InputTypeButtonActionView("");
            result.Widget.ClassList.Add(Magics.CssClassDatagridAction);
            var img = new HTMLImageElement {Src = Magics.IconUrlExit};
            result.Widget.AppendChild(img);
            return result;            
        }

        public static string CsrfToken { get; set; }
    }
}
