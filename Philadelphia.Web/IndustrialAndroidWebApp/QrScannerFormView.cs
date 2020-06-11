using System;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class QrScannerFormView : IFormView<HTMLElement> {
        public IView<HTMLElement>[] Actions => ActionsBuilder.For(Unpause, Cancel);
        public InputTypeButtonActionView Cancel { get; } = new InputTypeButtonActionView(new LabelDescr {
            Label = I18n.Translate("Back"),
            PreLabelIcon = Tuple.Create(IconFontType.FontAwesomeRegular, FontAwesomeRegular.IconWindowClose)
        });
        public InputTypeButtonActionView Unpause { get; } = new InputTypeButtonActionView(new LabelDescr {
            Label = I18n.Translate("Scan again"),
            PreLabelIcon = Tuple.Create(IconFontType.FontAwesomeSolid, FontAwesomeSolid.IconUndo)
        });
        public LabellessReadOnlyView Error { get; } = new LabellessReadOnlyView();
        public string Label { get; set; }
        public int PadMmFromTop { get; set; }
        public int HeightMm { get; set; }

        public RenderElem<HTMLElement>[] Render(HTMLElement parentContainer) {
            return new RenderElem<HTMLElement>[] {
                $"<div style='font-weight: bold; font-size: 30px; height: {PadMmFromTop*4.3}pt'>", Label, "</div>",
                $"<div style='width: 100%; height: {HeightMm*4.3}pt; background: #aaa'></div>",
                Error
            };
        }
    }
}
