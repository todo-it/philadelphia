using System;
using System.Collections.Generic;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class ModalDialogFormCanvas : IFormCanvas<HTMLElement> {
        private static void Debug(string m) => Logger.Debug(typeof(ModalDialogFormCanvas), m);
        private readonly HTMLElement _modalGlass, _actionsInFooter, _body, _header, _headerTitle;
        private bool _isDragging;
        private readonly HTMLElement _dialog;
        private readonly InputTypeButtonActionView _userClose;
        private Action _onUserClose;
        private readonly string _formId;

        public string Title {
            set {
                _headerTitle.TextContent = value;
                _headerTitle.SetAttribute(Magics.AttrDataInnerHtml, value);
            }
        }

        public HTMLElement Body { 
            set { 
                Logger.Debug(GetType(),$"ModalDialogFromCanvas(formId={_formId}): body setting");
                _body.RemoveAllChildren();
                _body.AppendChild(value);
            } 
        }

        public IEnumerable<HTMLElement> Actions { 
            set { 
                Logger.Debug(GetType(),$"ModalDialogFromCanvas(formId={_formId}): actions setting");
                _actionsInFooter.RemoveAllChildren();
                value.ForEach(x => _actionsInFooter.AppendChild(x));
            } 
        }

        public Action UserCancel {
            set {
                Debug($"ModalDialogFromCanvas(formId={_formId}) setting UserCancel. Will be null={value == null}, was null={_onUserClose == null}");
                
                _onUserClose = value;
            }
        }

        public ModalDialogFormCanvas() {
            _formId = UniqueIdGenerator.GenerateAsString();
            _modalGlass = DocumentUtil.CreateElementHavingClassName("div", GetType().FullNameWithoutGenerics());
            _dialog = Document.CreateElement("div");
            
            _dialog.SetValuelessAttribute(Magics.AttrDataFormContainer);
            _dialog.SetAttribute(Magics.AttrDataFormId, _formId);
            _dialog.SetBoolAttribute(Magics.AttrDataFormIsPopup, true);
            _dialog.SetValuelessAttribute(Magics.AttrDataFormIsShown);
            _dialog.SetValuelessAttribute(Magics.AttrDataFormIsCloseable);
            _dialog.AddEventListener(Magics.ProgramaticCloseFormEventName, () => _onUserClose?.Invoke());
            
            _body = Document.CreateElement("div");
            _body.SetAttribute(Magics.AttrDataFormId, _formId);
            _body.SetValuelessAttribute(Magics.AttrDataFormBody);
            
            _actionsInFooter = new HTMLDivElement();
            _actionsInFooter.SetAttribute(Magics.AttrDataFormId, _formId);
            _actionsInFooter.SetValuelessAttribute(Magics.AttrDataFormActions);
            
            _header = Document.CreateElement("div");
            _header.SetAttribute(Magics.AttrDataFormId, _formId);
            _header.SetValuelessAttribute(Magics.AttrDataFormHeader);

            _headerTitle = DocumentUtil.CreateElementHavingClassName("div", "headerTitle");
            
            _userClose = InputTypeButtonActionView.CreateFontAwesomeIconedAction(IconFontType.FontAwesomeSolid, FontAwesomeSolid.IconTimes);
            _userClose.Widget.ClassList.Add(Magics.CssClassHeaderClose);
            _userClose.Widget.AddEventListener(EventType.Click, () => _onUserClose?.Invoke());
            
            _modalGlass.AppendChild(_dialog);
            _dialog.AppendChild(_header);
            
            _header.AppendChild(_headerTitle);
            _header.AppendChild(DocumentUtil.CreateElementHavingClassName("span", Magics.CssClassFlexSpacer));
            _header.AppendChild(_userClose.Widget);
            
            _dialog.AppendChild(_body);
            _dialog.AppendChild(_actionsInFooter);
            MakeItDraggable(_header);
        }

        //theoretically I can use html5's events: dragenter, drag. Unfortunately drag event has screenX, clientX properties that are always zero (at least in FF)
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
            if (Document.Body.Contains(_modalGlass)) {
                Logger.Error(GetType(), "cannot show already shown dialog");
                return;
            }
            
            _dialog.SetBoolAttribute(Magics.AttrDataFormIsCloseable, _onUserClose != null);
            _dialog.SetBoolAttribute(Magics.AttrDataFormIsShown, true);
            Document.Body.AppendChild(_modalGlass);
            
            BuildFormFromElement(_modalGlass).FindAndFocusOnFirstItem();
        }

        public void Hide() {
            if (!Document.Body.Contains(_modalGlass)) {
                Logger.Error(GetType(), "cannot hide hidden dialog");
                return;
            }
            
            _dialog.SetBoolAttribute(Magics.AttrDataFormIsCloseable, false);
            _dialog.SetBoolAttribute(Magics.AttrDataFormIsShown, false);
            
            Document.Body.RemoveChild(_modalGlass);
        }
        
        public static FormDescr BuildFormFromElement(HTMLElement el) {
            var shouldBeDialog = el.Children[0]; //glass is parent
            return new FormDescr(
                shouldBeDialog, 
                shouldBeDialog.Children[1], 
                shouldBeDialog.Children[2]);
        }
    }
}
