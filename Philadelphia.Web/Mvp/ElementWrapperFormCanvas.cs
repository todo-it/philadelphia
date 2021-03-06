﻿using System;
using System.Collections.Generic;
using System.Linq;
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
    public class ElementWrapperFormCanvas : IHtmlFormCanvas {
        private static void Debug(string m) => Logger.Debug(typeof(ElementWrapperFormCanvas), m);

        private readonly HTMLElement _wrapped;
        private readonly HTMLElement _body,_actions,_extraElement;
        private readonly IActionView<HTMLElement> _maybeUserCancelUiAction;
        private Action _onUserClose;
        private LayoutModeType _layoutMode;
        public string FormId { get; } = UniqueIdGenerator.GenerateAsString();
        private readonly ITitleFormCanvasStrategy _titleImpl;

        public HTMLElement ContainerElement => _wrapped;
        public string Title { set => _titleImpl.Title =value; }

        public LayoutModeType LayoutMode {
            set {
                _wrapped.RemoveClasses(_layoutMode.GetAsCssClassName());
                _layoutMode = value;
                _wrapped.AddClasses(_layoutMode.GetAsCssClassName());
            }
        }

        public bool IsShown {
            get => _wrapped.GetBoolAttribute(Magics.AttrDataFormIsShown) == true;
            set => _wrapped.SetBoolAttribute(Magics.AttrDataFormIsShown, value);
        }

        public HTMLElement Body {
            set { 
                Logger.Debug(GetType(),$"ElementWrapperFormCanvas(formId={FormId}): body setting");
                _body.RemoveAllChildren();
                _body.AppendChild(value);
            } 
        }
        
        public IEnumerable<HTMLElement> Actions { 
            set { 
                Logger.Debug(GetType(),$"ElementWrapperFormCanvas(formId={FormId}): actions setting");

                FormCanvasShared.AddActions(
                    _actions,
                    value.ConcatElementIfTrue(_maybeUserCancelUiAction?.Enabled == true, _maybeUserCancelUiAction?.Widget));
            } 
        }

        public Action UserCancel {
            set {
                Debug($"ElementWrapperFormCanvas(formId={FormId}) setting UserCancel was closeable?={_onUserClose == null}, will be closeable?={value == null}");
                if (_maybeUserCancelUiAction != null) {
                    _maybeUserCancelUiAction.Enabled = value != null;
                }

                _onUserClose = value;
            }
        }

        public ElementWrapperFormCanvas(
                Func<IHtmlFormCanvas,ITitleFormCanvasStrategy> titleImpl,
                HTMLElement elementToWrap, Func<IActionView<HTMLElement>> nullOrCreateCloseButton,
                LayoutModeType layoutMode, HTMLElement extraElementOrNull=null) {
            
            _layoutMode = layoutMode;
            
            _wrapped = elementToWrap;
            _wrapped.RemoveAllChildren();
            _wrapped.AddClasses(GetType().FullNameWithoutGenerics(), _layoutMode.GetAsCssClassName());
            
            _wrapped.SetValuelessAttribute(Magics.AttrDataFormContainer);
            _wrapped.SetAttribute(Magics.AttrDataFormId, FormId);
            _wrapped.SetBoolAttribute(Magics.AttrDataFormIsPopup, false);
            IsShown = false;
            _wrapped.SetValuelessAttribute(Magics.AttrDataFormIsCloseable);
            
            _body = new HTMLDivElement();
            _body.SetAttribute(Magics.AttrDataFormId, FormId);
            _body.SetValuelessAttribute(Magics.AttrDataFormBody);
            
            _actions = new HTMLDivElement();
            _actions.SetAttribute(Magics.AttrDataFormId, FormId);
            _actions.SetValuelessAttribute(Magics.AttrDataFormActions);
            
            _maybeUserCancelUiAction = nullOrCreateCloseButton?.Invoke();
            
            _extraElement = extraElementOrNull ?? new HTMLSpanElement();
            _extraElement.AddClasses(Magics.CssClassExtraElement);
            
            _titleImpl = titleImpl(this);
        }

        private void OnUserClose() => _onUserClose?.Invoke();
        
        public void Show() {
            _titleImpl.OnCanvasShowing();
            
            _wrapped.AddEventListener(Magics.ProgramaticCloseFormEventName, OnUserClose);
            if (_maybeUserCancelUiAction != null) {
                _maybeUserCancelUiAction.Triggered += OnUserClose;    
            }

            _wrapped.AppendChild(_body);
            _wrapped.AppendChild(_actions);
            _wrapped.AppendChild(_extraElement);
            
            _wrapped.SetBoolAttribute(Magics.AttrDataFormIsCloseable, _onUserClose != null);
            IsShown = true;
        }

        public void Hide() {
            _wrapped.SetBoolAttribute(Magics.AttrDataFormIsCloseable, false);
            IsShown = false;
 
            _wrapped.RemoveEventListener(Magics.ProgramaticCloseFormEventName, OnUserClose);
            if (_maybeUserCancelUiAction != null) {
                _maybeUserCancelUiAction.Triggered -= OnUserClose;    
            }

            _titleImpl.OnCanvasHiding();
            _wrapped.RemoveAllChildren();
        }

        public void Focus() => AsFormDescr().FindAndFocusOnFirstItem();
        public FormDescr AsFormDescr() => BuildFormFromElement(ContainerElement);
        public static bool ContainsForm(HTMLElement el) => el.GetBoolAttribute(Magics.AttrDataFormIsShown) == true;
            
        public static FormDescr BuildFormFromElement(HTMLElement el) =>
            new FormDescr(
                el, 
                el.Children.First(x => x.HasAttribute(Magics.AttrDataFormBody)), 
                el.Children.First(x => x.HasAttribute(Magics.AttrDataFormActions)));
    }
}
