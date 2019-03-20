﻿using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Web;

namespace Philadelphia.Demo.Client {
    public class SomeChoicesFormView : IFormView<HTMLElement> {
        public IActionView<HTMLElement> First {get;} 
            = InputTypeButtonActionView.CreateFontAwesomeIconedButton("First", Magics.FontAwesomeChevronLeft);
        public IActionView<HTMLElement> Second {get;}
            = InputTypeButtonActionView.CreateFontAwesomeIconedButton("Second", Magics.FontAwesomeChevronRight);
        public IView<HTMLElement>[] Actions => new IView<HTMLElement>[]{First, Second};

        public RenderElem<HTMLElement>[] Render(HTMLElement _) {
            return new RenderElem<HTMLElement>[] {
                "<div style='text-align:center'>",
                "Click button<br>",
                "or close form<br>",
                "or press ESC to close this topmost dialog",
                "</div>"
            };
        }
    }
}