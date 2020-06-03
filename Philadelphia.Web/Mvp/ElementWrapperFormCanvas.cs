using System;
using System.Collections.Generic;
using Bridge.Html5;
using Philadelphia.Common;
// ReSharper disable InconsistentNaming

namespace Philadelphia.Web {
    public enum LayoutModeType {
        TitleExtra_Body_Actions,
        TitleExtra_Actions_Body,
        ExtraTitle_Body_Actions,
        Title_Body_ActionsExtra,
        ExtraActionsTitle_Body,
        ActionsTitleExtra_Body
    }

    public static class LayoutModeTypeExtensions {
        public static string GetAsCssClassName(this LayoutModeType self) {
            return nameof(LayoutModeType)+"_"+self.ToString();
        }
    }
    
    /// <summary>
    /// element is cleaned during adoption
    /// </summary>
    public class ElementWrapperFormCanvas : IFormCanvas<HTMLElement> {
        private static void Debug(string m) => Logger.Debug(typeof(ElementWrapperFormCanvas), m);

        private readonly HTMLElement _elementToWrap;
        private readonly HTMLElement _title,_body,_actions,_extraElement;
        private readonly IActionView<HTMLElement> _userCancelUiAction;
        private Action _onUserClose;
        private LayoutModeType _layoutMode;
        private readonly string _formId;

        public string Title {
            set {
                _title.RemoveAllChildren();
                if (string.IsNullOrEmpty(value)) {
                    return;
                }
                _title.AppendChild(new HTMLDivElement {TextContent = value});
            }
        }

        public LayoutModeType LayoutMode {
            set {
                _elementToWrap.RemoveClasses(_layoutMode.GetAsCssClassName());
                _layoutMode = value;
                _elementToWrap.AddClasses(_layoutMode.GetAsCssClassName());
            }
        }

        private bool IsShown {
            get => _elementToWrap.GetBoolAttribute(Magics.AttrDataFormIsShown) == true;
            set => _elementToWrap.SetBoolAttribute(Magics.AttrDataFormIsShown, value);
        }

        public HTMLElement Body {
            set { 
                Logger.Debug(GetType(),$"ElementWrapperFormCanvas(formId={_formId}): body setting");
                _body.RemoveAllChildren();
                _body.AppendChild(value);
            } 
        }
        
        public IEnumerable<HTMLElement> Actions { 
            set { 
                Logger.Debug(GetType(),$"ElementWrapperFormCanvas(formId={_formId}): actions setting");

                FormCanvasShared.AddActions(
                    _actions,
                    value.ConcatElementIfTrue(_userCancelUiAction.Enabled, _userCancelUiAction.Widget));
            } 
        }

        public Action UserCancel {
            set {
                Debug($"ElementWrapperFormCanvas(formId={_formId}) setting UserCancel was closeable?={_onUserClose == null}, will be closeable?={value == null}");
                _userCancelUiAction.Enabled = value != null;
                _onUserClose = value;
            }
        }

        public ElementWrapperFormCanvas(
                HTMLElement elementToWrap, Func<IActionView<HTMLElement>> createCloseButton,
                LayoutModeType layoutMode, HTMLElement extraElementOrNull=null) {

            _layoutMode = layoutMode;
            _formId = UniqueIdGenerator.GenerateAsString();
            
            _elementToWrap = elementToWrap;
            _elementToWrap.RemoveAllChildren();
            _elementToWrap.AddClasses(GetType().FullNameWithoutGenerics(), _layoutMode.GetAsCssClassName());
            
            _elementToWrap.SetValuelessAttribute(Magics.AttrDataFormContainer);
            _elementToWrap.SetAttribute(Magics.AttrDataFormId, _formId);
            _elementToWrap.SetBoolAttribute(Magics.AttrDataFormIsPopup, false);
            IsShown = false;
            _elementToWrap.SetValuelessAttribute(Magics.AttrDataFormIsCloseable);
            _elementToWrap.AddEventListener(Magics.ProgramaticCloseFormEventName, () => _onUserClose?.Invoke());
            
            //title needs to be in container as we need margin in styling. 
            //Margins are not reflected in neither ClientHeight nor OffsetHeight and one needs to use slow/unreliable
            //http://stackoverflow.com/questions/10787782/full-height-of-a-html-element-div-including-border-padding-and-margin
            
            _title  = new HTMLDivElement();
            _title.SetAttribute(Magics.AttrDataFormId, _formId);
            _title.SetValuelessAttribute(Magics.AttrDataFormTitle);
            
            _body = new HTMLDivElement();
            _body.SetAttribute(Magics.AttrDataFormId, _formId);
            _body.SetValuelessAttribute(Magics.AttrDataFormBody);
            
            _actions = new HTMLDivElement();
            _actions.SetAttribute(Magics.AttrDataFormId, _formId);
            _actions.SetValuelessAttribute(Magics.AttrDataFormActions);
            
            _userCancelUiAction = createCloseButton();
            _userCancelUiAction.Triggered += () => _onUserClose?.Invoke();
            
            _extraElement = extraElementOrNull ?? new HTMLSpanElement();
            _extraElement.AddClasses(Magics.CssClassExtraElement);
        }

        public void Show() {
            _elementToWrap.AppendChild(_title);
            _elementToWrap.AppendChild(_body);
            _elementToWrap.AppendChild(_actions);
            _elementToWrap.AppendChild(_extraElement);
            
            _elementToWrap.SetBoolAttribute(Magics.AttrDataFormIsCloseable, _onUserClose != null);
            IsShown = true;
            
            BuildFormFromElement(_elementToWrap).FindAndFocusOnFirstItem();
        }

        public void Hide() {
            _elementToWrap.SetBoolAttribute(Magics.AttrDataFormIsCloseable, false);
            IsShown = false;
            
            _elementToWrap.RemoveAllChildren();
        }

        public static FormDescr BuildFormFromElement(HTMLElement el) {
            return new FormDescr(el, el.Children[1], el.Children[2]);
        }
    }
}
