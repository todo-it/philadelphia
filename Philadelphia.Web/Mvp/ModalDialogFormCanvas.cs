using System;
using System.Collections.Generic;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class ModalDialogFormCanvas : IHtmlFormCanvas {
        private readonly HTMLElement _modalGlass, _actionsInFooter, _body, _header, _headerTitle, _dialog;
        private Action _onUserClose;
        private bool _isDragging;
        private readonly InputTypeButtonActionView _userClose;
        private readonly bool _clickingOnGlassDismissesDialog;

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

        public ModalDialogFormCanvas(bool clickingOnGlassDismissesDialog) {
            _clickingOnGlassDismissesDialog = clickingOnGlassDismissesDialog;
            _modalGlass = DocumentUtil.CreateElementHavingClassName("div", GetType().FullNameWithoutGenerics());
            _dialog = Document.CreateElement("div");
            
            _dialog.SetValuelessAttribute(Magics.AttrDataFormContainer);
            _dialog.SetAttribute(Magics.AttrDataFormId, FormId);
            _dialog.SetBoolAttribute(Magics.AttrDataFormIsPopup, true);
            IsShown = false;
            _dialog.SetValuelessAttribute(Magics.AttrDataFormIsCloseable);
            
            _body = Document.CreateElement("div");
            _body.SetAttribute(Magics.AttrDataFormId, FormId);
            _body.SetValuelessAttribute(Magics.AttrDataFormBody);
            
            _actionsInFooter = new HTMLDivElement();
            _actionsInFooter.SetAttribute(Magics.AttrDataFormId, FormId);
            _actionsInFooter.SetValuelessAttribute(Magics.AttrDataFormActions);
            
            _userClose = InputTypeButtonActionView.CreateFontAwesomeIconedAction(IconFontType.FontAwesomeSolid, FontAwesomeSolid.IconTimes);
            _userClose.Widget.ClassList.Add(Magics.CssClassHeaderClose);
            
            _modalGlass.AppendChild(_dialog);
            
            _header = Document.CreateElement("div");
            _header.SetAttribute(Magics.AttrDataFormId, FormId);
            _header.SetValuelessAttribute(Magics.AttrDataFormHeader);
            _headerTitle = DocumentUtil.CreateElementHavingClassName("div", Magics.CssClassHeaderTitle);
            
            _dialog.AppendChild(_header);
            
            _header.AppendChild(_headerTitle);
            _header.AppendChild(_userClose.Widget);
            
            _dialog.AppendChild(_body);
            _dialog.AppendChild(_actionsInFooter);
            
            MakeItDraggable(_header);
        }

        private void OnUserClose() => _onUserClose?.Invoke();
        
        public void Show() {
            if (Document.Body.Contains(_modalGlass)) {
                Logger.Error(GetType(), "cannot show already shown dialog");
                return;
            }
            
            _dialog.AddEventListener(Magics.ProgramaticCloseFormEventName, OnUserClose);
            _userClose.Widget.AddEventListener(EventType.Click, OnUserClose);
            if (_clickingOnGlassDismissesDialog) {
                _modalGlass.AddEventListener(EventType.Click, OnUserClose);
            }

            _dialog.SetBoolAttribute(Magics.AttrDataFormIsCloseable, _onUserClose != null);
            IsShown = true;
            Document.Body.AppendChild(_modalGlass);
        }

        public void Hide() {
            if (!Document.Body.Contains(_modalGlass)) {
                Logger.Error(GetType(), "cannot hide already hidden dialog");
                return;
            }
            
            if (_clickingOnGlassDismissesDialog) {
                _modalGlass.RemoveEventListener(EventType.Click, OnUserClose);
            }
            _userClose.Widget.RemoveEventListener(EventType.Click, OnUserClose);
            _dialog.RemoveEventListener(Magics.ProgramaticCloseFormEventName, OnUserClose);

            _dialog.SetBoolAttribute(Magics.AttrDataFormIsCloseable, false);
            IsShown = false;
            
            Document.Body.RemoveChild(_modalGlass);
        }
        
        public void Focus() => AsFormDescr().FindAndFocusOnFirstItem();
        public FormDescr AsFormDescr() => BuildFormFromElement(ContainerElement);

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
