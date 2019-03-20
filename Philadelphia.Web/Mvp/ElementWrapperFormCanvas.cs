using System;
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
    public class ElementWrapperFormCanvas : IFormCanvas<HTMLElement> {
        private static void Debug(string m) => Logger.Debug(typeof(ElementWrapperFormCanvas), m);

        private readonly HTMLElement _elementToWrap;
        private readonly HTMLElement _title,_body,_actions,_extraElement;
        private readonly IActionView<HTMLElement> _userCancel;
        private Action _onUserClose;
        private LayoutModeType _lastLayout = LayoutModeType.TitleExtra_Body_Actions;

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
                _elementToWrap.RemoveClasses(_lastLayout.GetAsCssClassName());
                _lastLayout = value;
                _elementToWrap.AddClasses(_lastLayout.GetAsCssClassName());
            }
        }
        public HTMLElement TitleElem => _title;
        public HTMLElement ExtraElement => _extraElement;

        public HTMLElement Body { 
            get {
                return _body;
            }
            set { 
                Logger.Debug(GetType(),"ElementWrapperFormCanvas: cleaning body before add");
                _body.RemoveAllChildren();
                Logger.Debug(GetType(),"ElementWrapperFormCanvas: appending element");
                _body.AppendChild(value);
            } 
        }
        public IEnumerable<HTMLElement> Actions { 
            set { 
                Logger.Debug(GetType(),"ElementWrapperFormCanvas: cleaning actions before add");
                _actions.RemoveAllChildren();
                Logger.Debug(GetType(),"ElementWrapperFormCanvas: appending actions");
                var actions = value.ToList();
                actions
                    .Where(x => !x.HasAttribute(Magics.AttrAlignToRight))
                    .ForEach(x => _actions.AppendChild(x));
                var toRight = actions.Where(x => x.HasAttribute(Magics.AttrAlignToRight)).ToList();

                if (_userCancel.Enabled) {
                    toRight.Add(_userCancel.Widget);
                }

                if (toRight.Any()) {
                    _actions.AppendChild(DocumentUtil.CreateElementHavingClassName("span", Magics.CssClassFlexSpacer));
                    toRight.ForEach(x => _actions.AppendChild(x));
                }
            } 
        }

        public Action UserCancel {
            set {
                Debug($"Setting UserCancel. Will be null={value == null}, was null={_onUserClose == null}");
                _userCancel.Enabled = value != null;

                if (_onUserClose != null) {
                    _userCancel.Triggered -= _onUserClose;
                }

                _onUserClose = value;

                if (_onUserClose != null) {
                    _userCancel.Triggered += _onUserClose;
                }
            }
        }

        public ElementWrapperFormCanvas(
                    HTMLElement elementToWrap, Func<IActionView<HTMLElement>> createCloseButton,
                    HTMLElement extraElementOrNull=null) {

            _userCancel = createCloseButton();
            _elementToWrap = elementToWrap;
            _extraElement = extraElementOrNull ?? new HTMLSpanElement();
            _extraElement.AddClasses(Magics.CssClassExtraElement);

            elementToWrap.RemoveAllChildren();
            
            if (_elementToWrap.Id == "") {
                _elementToWrap.Id = UniqueIdGenerator.GenerateAsString();
            }
            
            _elementToWrap.AddClasses(GetType().FullName, _lastLayout.GetAsCssClassName());
			
            elementToWrap.MarkAsFormView(false);

            //title needs to be in container as we need margin in styling. 
            //Margins are not reflected in neither ClientHeight nor OffsetHeight and one needs to use slow/unreliable
            //http://stackoverflow.com/questions/10787782/full-height-of-a-html-element-div-including-border-padding-and-margin
            
            _title = new HTMLDivElement {
                Id = UniqueIdGenerator.GenerateAsString(),
                ClassName = Magics.CssClassTitle };
            
            _body = new HTMLDivElement {
                Id = UniqueIdGenerator.GenerateAsString(),
                ClassName = Magics.CssClassBody };

            _actions = new HTMLDivElement {
                Id = UniqueIdGenerator.GenerateAsString(),
                ClassName = Magics.CssClassActions };
        }

        public void Show() {
            _elementToWrap.AppendChild(_title);
            _elementToWrap.AppendChild(_body);
            _elementToWrap.AppendChild(_actions);
            _elementToWrap.AppendChild(_extraElement);
        }

        public void Hide() => _elementToWrap.RemoveAllChildren();
    }
}
