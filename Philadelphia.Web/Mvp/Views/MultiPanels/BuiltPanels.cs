using System;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class BuiltPanels<T> {
        public T Panel {get; }
        public IFormRenderer<HTMLElement> First {get; }
        public IFormRenderer<HTMLElement> Second {get; }
        public ElementWrapperFormCanvas FirstCanvas {get;}
        public ElementWrapperFormCanvas SecondCanvas {get;}
        public Action HideAction { get; }
        public Action ShowAction { get; }

        private BuiltPanels(T panel, 
            IFormRenderer<HTMLElement> first, IFormRenderer<HTMLElement> second,
            ElementWrapperFormCanvas firstCanvas, ElementWrapperFormCanvas secondCanvas,
            Action hideAction, Action showAction) {
            
            Panel = panel;
            First = first;
            Second = second;
            FirstCanvas = firstCanvas;
            SecondCanvas = secondCanvas;

            HideAction = hideAction;
            ShowAction = showAction;
        }

        public static BuiltPanels<T> BuiltHideable(T panel, 
                IFormRenderer<HTMLElement> first, IFormRenderer<HTMLElement> second,
                ElementWrapperFormCanvas firstCanvas, ElementWrapperFormCanvas secondCanvas,
                Action hideAction, Action showAction) {
            
            return new BuiltPanels<T>(panel, first, second, firstCanvas, secondCanvas, hideAction, showAction);
        }

        public static BuiltPanels<T> BuiltNonHideable(T panel, 
                IFormRenderer<HTMLElement> first, IFormRenderer<HTMLElement> second,
                ElementWrapperFormCanvas firstCanvas, ElementWrapperFormCanvas secondCanvas) {
            
            return new BuiltPanels<T>(panel, first, second, firstCanvas, secondCanvas, null, null);
        }
    }
}
