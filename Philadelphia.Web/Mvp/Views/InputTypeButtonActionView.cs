using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
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

        /// <summary>
        /// create new element - sets up visuals and events
        /// </summary>
        public InputTypeButtonActionView(string labelContent, string preLabelContent = null) :
            this(Unit.Instance, CreateElement(labelContent, preLabelContent)) {}
        
        public static InputTypeButtonActionView CreateFontAwesomeIconedButton(
                string labelContent, string icon) {

            var res = new InputTypeButtonActionView(labelContent, icon);
            res.Widget.AddClasses(Magics.CssClassFontAwesomeBasedButton);
            return res;
        }

        public static InputTypeButtonActionView CreateFontAwesomeIconedButtonLabelless(string icon) {
            var res = new InputTypeButtonActionView("", icon);
            res.Widget.AddClasses(Magics.CssClassFontAwesomeBasedButtonLabelLess);
            return res;
        }

        public static InputTypeButtonActionView CreateFontAwesomeIconedAction(string fontAwesomeIcon, string cssClassName=Magics.CssClassAnchorWithFontIcon) {
            var action = new HTMLAnchorElement {
                ClassName = cssClassName,
                TextContent = fontAwesomeIcon
            };
            return new InputTypeButtonActionView(action, x => {
                if (x) {
                    action.SetAttribute("target", "_blank");
                    return;
                }
                action.RemoveAttribute("target");
            });
        }

        /// <summary>returns element AND action to set should-open-in-new-tab?</summary>
        private static Tuple<HTMLElement,Action<bool>,HTMLElement,HTMLElement> CreateElement(string labelContent, string preLabelContent = null) {
            var preLabel = new HTMLElement(ElementType.Span) {TextContent = preLabelContent ?? ""};
            var properLabel = new HTMLElement(ElementType.Span) {TextContent = labelContent};

            var result = new HTMLElement(ElementType.Span) {
                ClassName = typeof(InputTypeButtonActionView).FullName};
            result.SetAttribute("tabindex", "0");
            result.AppendChild(preLabel);
            result.AppendChild(properLabel);

            return Tuple.Create<HTMLElement,Action<bool>,HTMLElement,HTMLElement>(result, x => {
                if (x) {
                    result.SetAttribute("target", "_blank");
                    return;
                }
                result.RemoveAttribute("target");
            }, preLabel, properLabel);
        }

        public HTMLElement Widget => _elem;

        public static implicit operator RenderElem<HTMLElement>(InputTypeButtonActionView inp) {
            return RenderElem<HTMLElement>.Create(inp);
        }
    }
}
