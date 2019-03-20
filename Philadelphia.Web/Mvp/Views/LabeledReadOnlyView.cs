using System;
using System.Collections.Generic;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class LabeledReadOnlyView : IReadOnlyValueView<HTMLElement,string> {
        private readonly TextType _type;
        private readonly HTMLElement _elem, _label, _container;

        public event UiErrorsUpdated ErrorsChanged;
        public HTMLElement Widget => _container;
        public HTMLElement LabelElement => _label;
        public HTMLElement ContentElement => _elem;

        public string Value {
            get {
                switch (_type) {
                    case TextType.TreatAsPreformatted: return _elem.TextContent;
                    case TextType.TreatAsText: return _elem.TextContent;
                    case TextType.TreatAsHtml: return _elem.InnerHTML;
                    default: throw new Exception("unsupported TextType value");
                }
            }
            set {
                switch (_type) {
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

        public LabeledReadOnlyView(TextType type, string label, string containerType = "div", string elementType = "div", string defaultValue = null) {
            _type = type;
            if (_type == TextType.TreatAsPreformatted) {
                _elem.Style.WhiteSpace = WhiteSpace.Pre;
            }

            _container = DocumentUtil.CreateElementHavingClassName(containerType, GetType().FullName);
            _label = new HTMLLabelElement() {TextContent = label};
            _elem = new HTMLElement(elementType);
            if (defaultValue != null) {
                Value = defaultValue;    
            }
            
            _container.AppendChild(_label);
            _container.AppendChild(_elem);
        }

        public LabeledReadOnlyView(string label, string containerType = "div", string elementType = "div", string defaultValue = null) :
            this(TextType.TreatAsText, label, containerType, elementType, defaultValue) {
        }

        public ISet<string> Errors => DefaultInputLogic.GetErrors(_elem);

        public void SetErrors(ISet<string> errors, bool userChange) {
            _elem.SetAttribute(Magics.AttrDataErrorsTooltip, string.Join("\n", errors));
            _elem.Style.BackgroundColor = errors.Count <= 0 ? "" : "#ff0000";
            ErrorsChanged?.Invoke(this, errors);
        }

        public static implicit operator RenderElem<HTMLElement>(LabeledReadOnlyView inp) {
            return RenderElem<HTMLElement>.Create(inp);
        }
    }
}
