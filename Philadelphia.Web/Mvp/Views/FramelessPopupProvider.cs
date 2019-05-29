using System;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    /// <summary>needs to be placed within element having 'position: relative'</summary>
    public class FramelessPopupProvider : IView<HTMLElement> {
        private readonly HTMLElement _container = new HTMLSpanElement();
        private readonly HTMLElement _labelContainer = new HTMLSpanElement();
        private readonly HTMLElement _popupContainer = new HTMLSpanElement();
        private readonly HTMLElement _popup = new HTMLDivElement();
        
        public event Action<VisibilityAction> PopupStateChanged;

        public HTMLElement PopupHolderElement => _popup;

        public HTMLElement Widget => _container;
        public HTMLElement PopupRawContent {
            set {
                _popup.RemoveAllChildren();
                _popup.AppendChild(value);
            }
        }

        public IView<HTMLElement> PopupRichContent {
            set {
                _popup.RemoveAllChildren();
                _popup.AppendChild(value.Widget);
            }
        }

        public FramelessPopupProvider(InputTypeButtonActionView act) : this(act.Widget){
            act.Triggered += ShowPopup;
            act.StaysPressed = true;
            PopupStateChanged += x => {
                switch (x) {
                    case VisibilityAction.Hiding:
                        act.IsPressed = false;
                        break;
                    case VisibilityAction.Showing:
                        act.IsPressed = true;
                        break;
                }
            };
        }

        public FramelessPopupProvider(HTMLElement wrapsElement = null) {
            _container.ClassName = typeof(FramelessPopupProvider).FullName;

            if (wrapsElement != null) {
                _container.AppendChild(wrapsElement);
            }
            
            _popupContainer.AddClasses(Philadelphia.Web.Magics.CssClassPopupContainer);
            _container.AppendChild(_popupContainer);
            
            _popup.AddClasses(Philadelphia.Web.Magics.CssClassPopup);
            
            DocumentUtil.AddMouseClickListener(_container, ev => {
                if (!ev.HasHtmlTarget()) {
                    return;
                }
                
                if (ev.HtmlTarget().GetElementOrItsAncestorMatchingOrNull(x => _container == x) != null) {
                    return;
                }

                HidePopup();
            });
        }

        public void HidePopup() {
            if (_popupContainer.Contains(_popup)) {
                _popupContainer.RemoveChild(_popup);
                PopupStateChanged?.Invoke(VisibilityAction.Hiding);
            }
        }

        public void ShowPopup() {
            if (!_popupContainer.Contains(_popup)) {
                _popupContainer.AppendChild(_popup);
                PopupStateChanged?.Invoke(VisibilityAction.Showing);
            }
        }
    }
}
