using System;
using System.Collections.Generic;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class LabellessReadOnlyView : IReadOnlyValueView<HTMLElement,string> {
        private readonly TextType _inputType;
        private readonly HTMLElement _elem;

        public event UiErrorsUpdated ErrorsChanged;
        public HTMLElement Widget => _elem;

        public string Value {
            get {
                switch (_inputType) {
                    case TextType.TreatAsPreformatted: return _elem.TextContent;
                    case TextType.TreatAsText: return _elem.TextContent;
                    case TextType.TreatAsHtml: return _elem.InnerHTML;
                    default: throw new Exception("unsupported TextType value");
                }
            }
            set {
                switch (_inputType) {
                    case TextType.TreatAsPreformatted:
                        _elem.TextContent = value;
                        break;

                    case TextType.TreatAsText:
                        _elem.TextContent = value;
                        break;

                    case TextType.TreatAsHtml:
                        _elem.InnerHTML = value;
                        break;
                    default: throw new Exception("unsupported TextType value");
                }
            }
        }

        public LabellessReadOnlyView(string elementType="div", TextType inputType = TextType.TreatAsText, string defaultValue = null) {
            _inputType = inputType;
            
            _elem = DocumentUtil.CreateElementHavingClassName(elementType, GetType().FullName);
            if (_inputType == TextType.TreatAsPreformatted) {
                _elem.Style.WhiteSpace = WhiteSpace.Pre;
            }

            if (defaultValue != null) {
                Value = defaultValue;
            }
        }

        public ISet<string> Errors => DefaultInputLogic.GetErrors(_elem);

        public void SetErrors(ISet<string> errors, bool userChange) {
            _elem.SetAttribute(Magics.AttrDataErrorsTooltip, string.Join("\n", errors));
            _elem.Style.BackgroundColor = errors.Count <= 0 ? "" : "#ff0000";
            ErrorsChanged?.Invoke(this, errors);
        }
        
        public static implicit operator RenderElem<HTMLElement>(LabellessReadOnlyView inp) {
            return RenderElem<HTMLElement>.Create(inp);
        }
    }
}
