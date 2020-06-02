using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class FormDescr {
        //TODO consider adding API to be able to register custom IFormCanvas 
        public static IEnumerable<Tuple<Type,Func<HTMLElement,FormDescr>>> FormsTypes = new List<Tuple<Type,Func<HTMLElement,FormDescr>>> {
            Tuple.Create<Type,Func<HTMLElement,FormDescr>>(typeof(ElementWrapperFormCanvas), ElementWrapperFormCanvas.BuildFormFromElement),
            Tuple.Create<Type,Func<HTMLElement,FormDescr>>(typeof(ModalDialogFormCanvas), ModalDialogFormCanvas.BuildFormFromElement)
        };

        public HTMLElement FormContainerElement { get; }
        public HTMLElement BodyContainerElement { get; }
        public HTMLElement ActionsContainerElement { get; }

        public FormDescr(HTMLElement container, HTMLElement body, HTMLElement actions) {
            FormContainerElement = container;
            BodyContainerElement = body;
            ActionsContainerElement = actions;
        }

        public string FormId => FormContainerElement.GetAttribute(Magics.AttrDataFormId);
        public bool IsPopup => FormContainerElement.GetBoolAttribute(Magics.AttrDataFormIsPopup) == true;
        public bool IsCloseable => FormContainerElement.GetBoolAttribute(Magics.AttrDataFormIsCloseable) == true;
        public bool IsShown => FormContainerElement.GetBoolAttribute(Magics.AttrDataFormIsShown) == true;
        
        public HTMLElement DefaultButtonOrNull => ActionsContainerElement.Children.FirstOrDefault(x => x.HasAttribute(Magics.AttrDataFormDefaultAction));
        
        public void FindAndFocusOnFirstItem() {
            BodyContainerElement.TraverseUntilFirst(el => {
                if (el.TagName != "INPUT" && el.TagName != "TEXTAREA" && el.TagName != "SELECT") {
                    return false;
                }
                
                el.TryFocusElement();
                
                return true;
            });
        }
        
        public void InvokeClose() {
            if (!IsCloseable) {
                Logger.Error(GetType(), $"formId={FormId} is not closeable");
                return;
            }

            FormContainerElement.DispatchEvent(new Event(Magics.ProgramaticCloseFormEventName));
        }
        
        public static FormDescr GetFormContainerOrNull(HTMLElement formContainer) =>
            FormsTypes
                .Where(x => formContainer.ClassList.Contains(x.Item1.FullNameWithoutGenerics()))
                .Select(x => x.Item2(formContainer))
                .FirstOrDefault();
        
        public static FormDescr CreateFrom(HTMLElement formContainer) => 
            GetFormContainerOrNull(formContainer) ?? throw new Exception("cannot create FormDescr for given element. Unregistered FormCanvas class?");
    }

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
