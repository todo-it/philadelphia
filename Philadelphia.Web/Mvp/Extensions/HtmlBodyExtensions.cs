using System.Collections.Generic;
using System.Linq;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public static class HtmlBodyExtensions {
        public static List<FormDescr> GetAllForms(this HTMLBodyElement _) =>
            FormDescr.FormsTypes
                .SelectMany(x => Document.GetElementsByClassName(x.Item1.FullNameWithoutGenerics()).Select(FormDescr.CreateFrom))
                .ToList();

        public static FormDescr GetActiveFormOrNull(this HTMLBodyElement body) => body.GetAllForms().LastOrDefault(x => x.IsShown);

        public static FormDescr GetElementsFormOrNull(this HTMLElement self) {
            var maybeForm = FormDescr.GetFormContainerOrNull(self);
            if (maybeForm != null && maybeForm.IsShown) {
                return maybeForm;
            }

            self = self.ParentElement;

            return (self == null) ? null : self.GetElementsFormOrNull();
        }

        public static void ActivateMyFormsDefaultButtonIfAny(this HTMLElement self) {
            var form = self.GetElementsFormOrNull();
            if (form == null) {
                Logger.Debug(typeof(ElementExtensions), "Element doesn't seem to be contained in any form OR is detached from DOM");
                return;
            }
            
            Logger.Debug(typeof(ElementExtensions), "Element is in formId={0}", form.FormId);
            
            var button = form.DefaultButtonOrNull;
            if (button == null) {
                Logger.Debug(typeof(ElementExtensions), "Element's form doesn't seem to be have default button");
                return;
            }

            Logger.Debug(typeof(ElementExtensions), "Activating element's form default button");
            button.Click();
        }
    }
}
