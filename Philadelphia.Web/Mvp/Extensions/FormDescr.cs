using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class FormDescr {
        //TODO consider adding API to be able to register custom IFormCanvas 
        public static IEnumerable<Tuple<Type,Func<HTMLElement,bool>,Func<HTMLElement,FormDescr>>> FormsTypes = 
            new List<Tuple<Type,Func<HTMLElement,bool>,Func<HTMLElement,FormDescr>>> {
                Tuple.Create<Type,Func<HTMLElement,bool>,Func<HTMLElement,FormDescr>>(
                    typeof(ElementWrapperFormCanvas), 
                    ElementWrapperFormCanvas.ContainsForm, 
                    ElementWrapperFormCanvas.BuildFormFromElement),
                Tuple.Create<Type,Func<HTMLElement,bool>,Func<HTMLElement,FormDescr>>(
                    typeof(ModalDialogFormCanvas), 
                    ModalDialogFormCanvas.ContainsForm, 
                    ModalDialogFormCanvas.BuildFormFromElement)
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

        public bool FindAndFocusOnFirstItem() {
            var isSuccess = BodyContainerElement.TraverseUntilFirst(el => {
                if (el.TagName != "INPUT" && el.TagName != "TEXTAREA" && el.TagName != "SELECT") {
                    return false;
                }
                
                el.TryFocusElement();
                
                return true;
            });
            
            Logger.Debug(typeof(FormDescr), "FindAndFocusOnFirstItem() outcome={0}", isSuccess != null);
            return isSuccess != null;
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
                .Where(x => x.Item2(formContainer))
                .Select(x => x.Item3(formContainer))
                .FirstOrDefault();
        
        public static FormDescr CreateFrom(HTMLElement formContainer) => 
            GetFormContainerOrNull(formContainer) ?? throw new Exception("cannot create FormDescr for given element. Unregistered FormCanvas class?");
    }
}
