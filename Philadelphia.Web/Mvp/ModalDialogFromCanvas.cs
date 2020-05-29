using System;
using System.Collections.Generic;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class ModalDialogFromCanvas : IFormCanvas<HTMLElement> {
        private static void Debug(string m) => Logger.Debug(typeof(ModalDialogFromCanvas), m);
        private readonly HTMLElement _modalGlass, _footer, _body, _header, _headerTitle;
        private bool _shown;
        private bool _isDragging;
        private readonly HTMLElement _dialog;
        private readonly InputTypeButtonActionView _userClose;
        private Action _onUserClose;

        public string Title {
            set {
                _headerTitle.TextContent = value;
                _headerTitle.SetAttribute(Magics.AttrDataInnerHtml, value);
            }
        }

        public HTMLElement Body { 
            get {
                return _body.FirstElementChild;
            }
            set { 
                _body.RemoveAllChildren();
                _body.AppendChild(value);
            } 
        }

        public IEnumerable<HTMLElement> Actions { 
            set { 
                _footer.RemoveAllChildren();
                value.ForEach(x => _footer.AppendChild(x));
            } 
        }

        public Action UserCancel {
            set {
                Debug($"Setting UserCancel. Will be null={value == null}, was null={_onUserClose == null}");
                //adding or removing action widget
                if (_onUserClose == null && value != null) {
                    _header.AppendChild(_userClose.Widget); 
                } else if (_onUserClose != null && value == null) {
                    _header.RemoveChild(_userClose.Widget);
                }

                if (_onUserClose != null) {
                    _userClose.Triggered -= _onUserClose;
                }

                _onUserClose = value;

                if (_onUserClose != null) {
                    _userClose.Triggered += _onUserClose;
                }
            }
        }

        public ModalDialogFromCanvas() {
            _modalGlass = DocumentUtil.CreateElementHavingClassName("div", GetType().FullName);
            _dialog = Document.CreateElement("div");
            _dialog.MarkAsFormView(true);
            _header = DocumentUtil.CreateElementHavingClassName("div", "header");
            _body = DocumentUtil.CreateElementHavingClassName("div", Magics.CssClassBody);
            _footer = DocumentUtil.CreateElementHavingClassName("div", Magics.CssClassActions);

            _headerTitle = DocumentUtil.CreateElementHavingClassName("div", "headerTitle");
            
            _userClose = InputTypeButtonActionView.CreateFontAwesomeIconedAction(IconFontType.FontAwesomeSolid, FontAwesomeSolid.IconTimes);
            _userClose.Widget.ClassList.Add(Magics.CssClassHeaderClose);
            _userClose.Widget.SetAttribute(Magics.AttrDataEscListener, "");

            _modalGlass.AppendChild(_dialog);
            _dialog.AppendChild(_header);
            _header.AppendChild(_headerTitle);
            _header.AppendChild(DocumentUtil.CreateElementHavingClassName("span", Magics.CssClassFlexSpacer));
            _dialog.AppendChild(_body);
            _dialog.AppendChild(_footer);
            MakeItDraggable(_header);
        }

        //theoretically I can use html5's events: dragenter, drag. Unfortunatelly drag event has screenX, clientX properties that are always zero (at least in FF)
        private void MakeItDraggable(HTMLElement header) {
            DocumentUtil.AddMouseDownListener(header, x => {
                if (!x.HasHtmlTarget()) {
                    return;
                }
                var htmlTarget = x.HtmlTarget();

                //clicked on header OR on title in a header BUT not on close button
                if (_userClose.Widget != htmlTarget && 
                    htmlTarget.IsElementOrItsDescendant(header)) {
                    Logger.Debug(GetType(), "potential dragging started");
                    _isDragging = true;    
                }
            });

            //as it is not raised if event happens while mouse doens't hover over element
            DocumentUtil.AddMouseUpListener(header, _ => {
                Logger.Debug(GetType(), "potential dragging stopped");
                _isDragging = false;
            });

            //as it is not raised if event happens while mouse doesn't hover over element
            DocumentUtil.AddMouseMoveListener(header, ev => {
                //_Logger.Debug(GetType(), "dragging? {0}", _isDragging);
                if (!_isDragging) {
                    return;
                }
                ev.PreventDefault();
                
                _dialog.Style.Position = Position.Absolute;
                
                var posDialog = _dialog.GetBoundingClientRect();
                var posHeader = _header.GetBoundingClientRect();
                
                _dialog.Style.Left = string.Format("{0}px", ev.PageX - posDialog.Width/2);
                _dialog.Style.Top = string.Format("{0}px", ev.PageY - posHeader.Height/2);
            });
        }

        public void Show() {
            if (!_shown) {
                Document.Body.AppendChild(_modalGlass);
                _shown = true;
            } else {
                Logger.Error(GetType(), "cannot show already shown dialog");
                throw new Exception("cannot show already shown dialog");
            }
        }

        public void Hide() {
            if (_shown) {
                Document.Body.RemoveChild(_modalGlass);
                _shown = true;
            } else {
                Logger.Error(GetType(), "cannot hide hidden dialog");
                throw new Exception("cannot hide already hidden dialog");
            }
        }
    }
}
