using System;
using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Web;

namespace Philadelphia.Demo.Client {
    public class SomeChoicesFormView : IFormView<HTMLElement> {
        public IActionView<HTMLElement> First {get;} 
            = new InputTypeButtonActionView(new LabelDescr {
                PreLabelIcon = Tuple.Create(IconFontType.FontAwesomeSolid, FontAwesomeSolid.IconChevronLeft),
                Label = "First"
            });
        public IActionView<HTMLElement> Second {get;}
            = new InputTypeButtonActionView(
                new LabelDescr {
                    PreLabelIcon = Tuple.Create(IconFontType.FontAwesomeSolid, FontAwesomeSolid.IconChevronRight),
                    Label = "Second" 
                }, 
                LeftOrRight.Right)
                    .With(x => x.Widget.SetValuelessAttribute(Magics.AttrAlignToRight));
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
