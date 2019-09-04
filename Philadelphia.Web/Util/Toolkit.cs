﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bridge;
using Philadelphia.Common;
using Bridge.Html5;

namespace Philadelphia.Web {
    public static class Toolkit {
        public static void InitializeToolkit(
                ILoggerImplementation customLogger = null, 
                Func<DatagridContent,Task<FileModel>> spreadsheetBuilder = null) {

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
            } else if (DocumentUtil.HasHashParameter("debug") && DocumentUtil.GetHashParameter("debug") == "lite") {
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
            } else if (DocumentUtil.HasHashParameter("debug")) {
                Logger.ConfigureImplementation(new ForwardEverythingToConsoleLogLoggerImplementation());
            } else {
                Logger.ConfigureImplementation(new ForwardErrorsToConsoleLogLoggerImplementation());
            }
            
            DocumentUtil.Initialize(); //initialize tooltips, esc handler etc
            
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
        }

        public static BaseFormRenderer DefaultFormRenderer(LayoutModeType? defaultLayout = null) {
            var documentBodyCanvas = new ElementWrapperFormCanvas(Document.Body, DefaultExitButtonBuilder);
            
            if (defaultLayout.HasValue) {
                documentBodyCanvas.LayoutMode = defaultLayout.Value;
            }
            
            var popupProvider = new FactoryMethodProvider<IFormCanvas<HTMLElement>>(() => new ModalDialogFromCanvas());
            
            return new BaseFormRenderer(documentBodyCanvas, popupProvider);
        }

        public static CalculateTbodyHeight DefaultTableBodyHeightProvider(int fixedHeightToAdd=0) {
             return (el,theaderHeight,_) => el.GetAvailableHeightForFormElement(0, 2) - theaderHeight + fixedHeightToAdd;
        }

        public static IActionView<HTMLElement> DefaultExitButtonBuilder() {
            var result = new InputTypeButtonActionView("");
            result.Widget.ClassList.Add(Magics.CssClassDatagridAction);
            var img = new HTMLImageElement {
                Src = Magics.IconUrlExit
            };
            result.Widget.AppendChild(img);
            return result;            
        }

        public static string CsrfToken { get; set; }
    }
}
