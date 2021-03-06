using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public enum LeftOrRight {
        Left,
        Right
    }

    public class InputTypeButtonActionView : IActionView<HTMLElement> {
        private readonly HTMLElement _elem,_properLabelElem,_preLabelElem;
        private Action<bool> _opensNewTabImpl;
        private bool _staysPressed;
        public event Action Triggered;

        public HTMLElement ProperLabelElem => _properLabelElem;
        public HTMLElement PreLabelElem => _preLabelElem;

        public bool IsPressed {
            get => _elem.ClassList.Contains(Magics.CssClassPressed);
            set => _elem.AddOrRemoveClass(value, Magics.CssClassPressed);
        }
        public bool StaysPressed {
            get => _staysPressed;
            set {
                _staysPressed = value; 
                if (!value) {
                    Widget.RemoveClasses(Magics.CssClassPressed);    
                }
            }
        }

        public bool Enabled { 
            get => _elem.ClassList.Contains(Magics.CssClassEnabled);
            set => _elem.AddOrRemoveClass(value, Magics.CssClassEnabled);
        }

        public ISet<string> DisabledReason { 
            set { 
                if (!value.Any()) {
                    _elem.RemoveAttribute(Magics.AttrDataDisabledTooltip);
                } else {
                    _elem.SetAttribute(Magics.AttrDataDisabledTooltip, string.Join("\n", value));
                }
                
            } 
        }

        public bool OpensNewTab {
            set => _opensNewTabImpl?.Invoke(value);
        }

        public string ErrorTooltip {
            set {
                if (string.IsNullOrEmpty(value)) {
                    _elem.RemoveAttribute(Magics.AttrDataErrorsTooltip);
                } else {
                    _elem.SetAttribute(Magics.AttrDataErrorsTooltip, value);
                }
            }
        }

        private ActionViewState _state = ActionViewState.CreateIdleOrSuccess();
        public ActionViewState State {
            set {
                _state = value;

                switch (value.Type) {
                    case ActionViewStateType.OperationFailed:
                        _elem.ClassList.Remove(Magics.CssClassRunning);
                        _elem.ClassList.Add(Magics.CssClassFailed);
                        ErrorTooltip = value.ErrorOrNull.Message;
                        break;

                    case ActionViewStateType.OperationRunning:
                        _elem.ClassList.Add(Magics.CssClassRunning);
                        _elem.ClassList.Remove(Magics.CssClassFailed);
                        ErrorTooltip = null;
                        break;

                    case ActionViewStateType.IdleOrSuccess:
                        _elem.ClassList.Remove(Magics.CssClassRunning);
                        _elem.ClassList.Remove(Magics.CssClassFailed);
                        ErrorTooltip = null;
                        break;
                }
            }
        }

        // has first dummy parameter to gain different constructor signature
        private InputTypeButtonActionView(Unit _, Tuple<HTMLElement,Action<bool>,HTMLElement,HTMLElement> elAndShouldOpenInNewTab) {
            _elem = elAndShouldOpenInNewTab.Item1;
            Enabled = Enabled; //initialized CSS class
            _opensNewTabImpl = elAndShouldOpenInNewTab.Item2;
            _preLabelElem = elAndShouldOpenInNewTab.Item3;
            _properLabelElem = elAndShouldOpenInNewTab.Item4;
            _elem.ClassList.Add(Magics.CssClassEnabled);

            _elem.OnClick += x => {
                // it is responsibility of model to check if action is enabled...
                Logger.Debug(GetType(), "InputTypeButtonActionView clicked. enabled?={0} hasListeners?{1}", Enabled, Triggered != null);
                
                if (!Enabled || _state.Type == ActionViewStateType.OperationRunning) {
                    return;
                }

                if (StaysPressed) {
                    IsPressed = !IsPressed;
                    _elem.AddOrRemoveClass(IsPressed, Magics.CssClassPressed);
                }

                x.PreventDefault(); //if wrapped anchor then it should not really navigate to it
                _elem.Blur(); //so that global ENTER handler won't be called first on this button
                Triggered?.Invoke();
            };
            
            _elem.OnKeyDown += ev => {
                if (ev.KeyCode == Magics.KeyCodeEnter) {
                    Logger.Debug(GetType(), "Handling ENTER key");
                    _elem.Blur(); //so that global ENTER handler won't be called first on this button
                    Triggered?.Invoke();
                }
            };
        }

        /// <summary>
        /// wrap existing element - don't change visuals, just setup events
        /// </summary>
        /// <param name="elem"></param>
        public InputTypeButtonActionView(HTMLElement elem, Action<bool> shouldOpenInNewTabImpl) : 
            //FIXME: optionally support getter for label element
            this(Unit.Instance, Tuple.Create(elem, shouldOpenInNewTabImpl, (HTMLElement)null, (HTMLElement)null)) {}
        
        /// <summary>
        /// wrap existing element - don't change visuals, just setup events
        /// </summary>
        /// <param name="elem"></param>
        public InputTypeButtonActionView(HTMLElement elem) : 
            //FIXME: optionally support getter for label element
            this(Unit.Instance, Tuple.Create<HTMLElement,Action<bool>,HTMLElement,HTMLElement>(elem, _ => {}, null, null)) {}

        //most commonly used overload
        public InputTypeButtonActionView(string labelContent, LeftOrRight loc = LeftOrRight.Left) :
            this(Unit.Instance, CreateElement(labelContent, null, loc)) {}

        public InputTypeButtonActionView(LabelDescr lbl, LeftOrRight loc = LeftOrRight.Left) :
            this(Unit.Instance, CreateElement(
                lbl.Label, 
                lbl.PreLabelIcon != null ? Tuple.Create(lbl.PreLabelIcon.Item1, lbl.PreLabelIcon.Item2) : null, 
                loc)) {}

        
        /// <summary>
        /// create new element - sets up visuals and events
        /// </summary>
        [Obsolete("use constructor accepting LabelDescr parameter")]
        public InputTypeButtonActionView(string labelContent, Tuple<IconFontType,string> preLabel, LeftOrRight loc = LeftOrRight.Left) :
            this(Unit.Instance, CreateElement(labelContent, preLabel, loc)) {}

        [Obsolete("use constructor accepting LabelDescr parameter")]
        public static InputTypeButtonActionView CreateFontAwesomeIconedButton(
                IconFontType font, string labelContent, string icon, LeftOrRight loc = LeftOrRight.Left) {

            var res = new InputTypeButtonActionView(labelContent, Tuple.Create(font, icon), loc);
            return res;
        }

        [Obsolete("use constructor accepting LabelDescr parameter")]
        public static InputTypeButtonActionView CreateFontAwesomeIconedButtonLabelless(IconFontType font, string icon) {
            var res = new InputTypeButtonActionView(null, Tuple.Create(font, icon));
            return res;
        }

        [Obsolete("use constructor accepting LabelDescr parameter")]
        public static InputTypeButtonActionView CreateFontAwesomeIconedAction(
                IconFontType font, string fontAwesomeIcon, string cssClassName=Magics.CssClassAnchorWithFontIcon) {
            
            var action = new HTMLAnchorElement {TextContent = fontAwesomeIcon};
            action.AddClasses(cssClassName, font.ToCssClassName());
            
            return new InputTypeButtonActionView(action, x => {
                if (x) {
                    action.SetAttribute("target", "_blank");
                    return;
                }
                action.RemoveAttribute("target");
            });
        }

        /// <summary>returns element AND action to set should-open-in-new-tab?</summary>
        private static Tuple<HTMLElement,Action<bool>,HTMLElement,HTMLElement> CreateElement(
                string labelContent, Tuple<IconFontType, string> preLabel = null, LeftOrRight loc = LeftOrRight.Left) {

            var hasIcon = false;
            var hasLabel = false;
            
            var preLblEl = new HTMLElement(ElementType.Span);
            if (preLabel != null) {
                preLblEl.TextContent = preLabel.Item2;
                preLblEl.AddClasses(preLabel.Item1.ToCssClassName());
                hasIcon = true;
            }
            
            var properLabel = new HTMLElement(ElementType.Span);
            if (!string.IsNullOrEmpty(labelContent)) {
                properLabel.TextContent = labelContent;
                hasLabel = true;
            }

            var result = new HTMLElement(ElementType.Span);
            
            result.AppendChild(preLblEl);
            result.AppendChild(properLabel);
            
            result.SetAttribute("tabindex", "0");
            
            result.AddClasses(typeof(InputTypeButtonActionView).FullName);
            if (loc == LeftOrRight.Left) {
                result.AddClasses(Magics.CssClassOrderIsIconThenLabel);
            }

            if (hasIcon && hasLabel) {
                result.AddClasses(Magics.CssClassHasIconAndLabel);
            } else if (hasIcon) { //labelless
                result.AddClasses(Magics.CssClassHasIconWithoutLabel);
            } else { //label only
                result.AddClasses(Magics.CssClassHasNoIconHasLabel);
            }

            return Tuple.Create<HTMLElement,Action<bool>,HTMLElement,HTMLElement>(result, x => {
                if (x) {
                    result.SetAttribute("target", "_blank");
                    return;
                }
                result.RemoveAttribute("target");
            }, preLblEl, properLabel);
        }

        public HTMLElement Widget => _elem;

        public static implicit operator RenderElem<HTMLElement>(InputTypeButtonActionView inp) {
            return RenderElem<HTMLElement>.Create(inp);
        }
    }
}
