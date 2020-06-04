using System;
using System.Collections.Generic;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class ModalDialogFormCanvas : IHtmlFormCanvas {
        private readonly HTMLElement _modalGlass, _actionsInFooter, _body, _header, _headerTitle, _dialog;
        private Action _onUserClose;
        private bool _isDragging;
        
        public string FormId { get; } = UniqueIdGenerator.GenerateAsString();
        public string Title {
            set {
                _headerTitle.TextContent = value;
                _headerTitle.SetAttribute(Magics.AttrDataInnerHtml, value);
            }
        }
        public HTMLElement ContainerElement => _modalGlass;

        public bool IsShown {
            get => _dialog.GetBoolAttribute(Magics.AttrDataFormIsShown) == true;
            set => _dialog.SetBoolAttribute(Magics.AttrDataFormIsShown, value);
        }

        public HTMLElement Body { 
            set { 
                Logger.Debug(GetType(), $"ModalDialogFromCanvas(formId={FormId}): body setting");
                _body.RemoveAllChildren();
                _body.AppendChild(value);
            } 
        }

        public IEnumerable<HTMLElement> Actions { 
            set { 
                Logger.Debug(GetType(),$"ModalDialogFromCanvas(formId={FormId}): actions setting");
                FormCanvasShared.AddActions(_actionsInFooter, value);
            } 
        }

        public Action UserCancel {
            set {
                Logger.Debug(GetType(), $"ModalDialogFromCanvas(formId={FormId}) setting UserCancel. Will be null={value == null}, was null={_onUserClose == null}");
                _onUserClose = value;
            }
        }

        public ModalDialogFormCanvas() {
            _modalGlass = DocumentUtil.CreateElementHavingClassName("div", GetType().FullNameWithoutGenerics());
            _dialog = Document.CreateElement("div");
            
            _dialog.SetValuelessAttribute(Magics.AttrDataFormContainer);
            _dialog.SetAttribute(Magics.AttrDataFormId, FormId);
            _dialog.SetBoolAttribute(Magics.AttrDataFormIsPopup, true);
            IsShown = false;
            _dialog.SetValuelessAttribute(Magics.AttrDataFormIsCloseable);
            _dialog.AddEventListener(Magics.ProgramaticCloseFormEventName, () => _onUserClose?.Invoke());
            
            _body = Document.CreateElement("div");
            _body.SetAttribute(Magics.AttrDataFormId, FormId);
            _body.SetValuelessAttribute(Magics.AttrDataFormBody);
            
            _actionsInFooter = new HTMLDivElement();
            _actionsInFooter.SetAttribute(Magics.AttrDataFormId, FormId);
            _actionsInFooter.SetValuelessAttribute(Magics.AttrDataFormActions);
            
            var userClose = InputTypeButtonActionView.CreateFontAwesomeIconedAction(IconFontType.FontAwesomeSolid, FontAwesomeSolid.IconTimes);
            userClose.Widget.ClassList.Add(Magics.CssClassHeaderClose);
            userClose.Widget.AddEventListener(EventType.Click, () => _onUserClose?.Invoke());
            
            _modalGlass.AppendChild(_dialog);
            
            _header = Document.CreateElement("div");
            _header.SetAttribute(Magics.AttrDataFormId, FormId);
            _header.SetValuelessAttribute(Magics.AttrDataFormHeader);
            _headerTitle = DocumentUtil.CreateElementHavingClassName("div", Magics.CssClassHeaderTitle);
            
            _dialog.AppendChild(_header);
            
            _header.AppendChild(_headerTitle);
            _header.AppendChild(userClose.Widget);
            
            _dialog.AppendChild(_body);
            _dialog.AppendChild(_actionsInFooter);
            
            MakeItDraggable(_header);
        }

        public void Show() {
            if (Document.Body.Contains(_modalGlass)) {
                Logger.Error(GetType(), "cannot show already shown dialog");
                return;
            }
            
            _dialog.SetBoolAttribute(Magics.AttrDataFormIsCloseable, _onUserClose != null);
            IsShown = true;
            Document.Body.AppendChild(_modalGlass);
            
            BuildFormFromElement(_modalGlass).FindAndFocusOnFirstItem();
        }

        public void Hide() {
            if (!Document.Body.Contains(_modalGlass)) {
                Logger.Error(GetType(), "cannot hide hidden dialog");
                return;
            }
            
            _dialog.SetBoolAttribute(Magics.AttrDataFormIsCloseable, false);
            IsShown = false;
            
            Document.Body.RemoveChild(_modalGlass);
        }
        
        public static FormDescr BuildFormFromElement(HTMLElement el) {
            var shouldBeDialog = el.Children[0]; //glass is parent
            return new FormDescr(
                shouldBeDialog, 
                shouldBeDialog.Children[1], 
                shouldBeDialog.Children[2]);
        }
        
        //theoretically I can use html5's events: dragenter, drag. Unfortunately drag event has screenX, clientX properties that are always zero (at least in FF)
        private void MakeItDraggable(HTMLElement header) {
            DocumentUtil.AddMouseDownListener(header, x => {
                if (!x.HasHtmlTarget()) {
                    return;
                }
                var htmlTarget = x.HtmlTarget();

                //clicked on header OR on title in a header BUT not on close button
                if (!htmlTarget.ClassList.Contains(Magics.CssClassHeaderClose) && //not clicked on user close 
                    htmlTarget.IsElementOrItsDescendant(header)) {
                    Logger.Debug(GetType(), "potential dragging started");
                    _isDragging = true;    
                }
            });

            //needed as dragging event is not raised when mouse doesn't hover over the element
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
    }
}
