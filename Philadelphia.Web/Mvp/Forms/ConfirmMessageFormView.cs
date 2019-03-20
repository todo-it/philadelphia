using System;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public enum ConfirmLabels {
        ConfirmCancel,
        YesNo,
        OkCancel
    }

    public static class ConfirmLabelsExtensions {
        public static string GetConfirmLabel(this ConfirmLabels self) {
            switch (self) {
                case ConfirmLabels.ConfirmCancel: return I18n.Translate("Confirm");
                case ConfirmLabels.YesNo: return I18n.Translate("Yes");
                case ConfirmLabels.OkCancel: return I18n.Translate("OK");
                default: throw new Exception("unsupported ConfirmLabels");
            }
        }

        public static string GetCancelLabel(this ConfirmLabels self) {
            switch (self) {
                case ConfirmLabels.ConfirmCancel: return I18n.Translate("Cancel");
                case ConfirmLabels.YesNo: return I18n.Translate("No");
                case ConfirmLabels.OkCancel: return I18n.Translate("Cancel");
                default: throw new Exception("unsupported ConfirmLabels");
            }
        }
    }

    public class ConfirmMessageFormView : IFormView<HTMLElement> {
        public LabellessReadOnlyView Message { get; }
        public InputTypeButtonActionView Confirm { get; }
        public InputTypeButtonActionView Cancel { get; }
        
        public ConfirmLabels LabelsType {get; set; }

        public ConfirmMessageFormView(
                TextType inputType = TextType.TreatAsText, ConfirmLabels type = ConfirmLabels.ConfirmCancel) {

            LabelsType = type;
            Message = new LabellessReadOnlyView("div", inputType);
            Confirm = new InputTypeButtonActionView(LabelsType.GetConfirmLabel())
                .With(x => x.MarkAsFormsDefaultButton());
            Cancel = new InputTypeButtonActionView(LabelsType.GetCancelLabel());
        }

        public RenderElem<HTMLElement>[] Render(HTMLElement _) {
            return new RenderElem<HTMLElement>[] {"<div>", Message, "</div>"};
        }

        public IView<HTMLElement>[] Actions => new IView<HTMLElement>[] {Confirm,Cancel};
    }
}
