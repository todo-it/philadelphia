using System;
using System.Collections.Generic;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class AnchorBasedActionView : IActionView<HTMLElement> {
        private readonly HTMLAnchorElement _a;
        public HTMLElement Widget => _a;
        public event Action Triggered;

        private bool _enabled = true; 
        public bool Enabled {
            get {
                return _enabled;
            }
            set {
                _enabled = value;

                var effective = value && _curState.Type != ActionViewStateType.OperationRunning;
                _a.AddOrRemoveClass(effective, Magics.CssClassEnabled);
                _a.AddOrRemoveClass(!effective, Magics.CssClassDisabled);
                _a.Style.TextDecoration = effective ? TextDecoration.Underline : TextDecoration.None;
                _a.Style.Cursor = effective ? Cursor.Pointer : Cursor.Default;
            }
        }
        public bool OpensNewTab { set { throw new Exception("not supported");} }
        public bool IsPressed { get { return false; } }
        public bool StaysPressed  { set { throw new Exception("not supported");} }
        public ISet<string> DisabledReason { set { DefaultInputLogic.SetDisabledReasons(_a, value); } }
        public string Label { set { _a.TextContent = value; } }
        public string Title { set { _a.Title = value; } }
        public string Href { set { _a.Href = value; } }
        public string Target { set { _a.Target = value; } }
        public Func<HTMLElement,bool> ShouldTriggerOnTarget {get; set; } = _ => true;

        private ActionViewState _curState = ActionViewState.CreateIdleOrSuccess();
        public ActionViewState State {
            get {
                return _curState;
            }
            set {
                _curState = value;
                Enabled = Enabled; //set css classes

                HashSet<string> errors;

                switch (value.Type) {
                    case ActionViewStateType.IdleOrSuccess:
                        errors = new HashSet<string>();
                        break;

                    case ActionViewStateType.OperationRunning:
                        errors = new HashSet<string>();
                        break;

                    case ActionViewStateType.OperationFailed:
                        var err = value.ErrorOrNull?.Message ?? I18n.Translate("unknown error");
                        errors = new HashSet<string>(new [] {err});
                        break;

                    default: throw new Exception("unsupported ActionViewStateType");
                }

                DefaultInputLogic.SetErrorsTooltip(_a, errors);
            }
        }

        public AnchorBasedActionView(string textContent = null, string title = null) {
            _a = new HTMLAnchorElement {
                Href = "#",
                ClassName = GetType().FullNameWithoutGenerics(),
                Title = title ?? "" };

            if (textContent != null) {
                _a.TextContent = textContent;
            }

            Enabled = true; //initialize css class

            _a.OnClick += ev => {
                if (ev.HasHtmlTarget() && !ShouldTriggerOnTarget(ev.HtmlTarget())) {
                    return;
                }

                if (_a.Href == "#" || !Enabled) {
                    ev.PreventDefault();
                }
                
                if (!Enabled || State.Type == ActionViewStateType.OperationRunning) {
                    return;
                }

                Triggered?.Invoke();
            };
        }
    }
}
