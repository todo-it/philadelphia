using System;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    /// <summary>
    /// provides ability for nested elements to be notified that mouse action (such as click) happened outside of this element. 
    /// Why? Example: on-hover-activated-menu may want to be hidden/collapsed
    /// Such listener needs to be registered on BODY but in a way that is garbage collectable
    /// </summary>
    public class MouseObserver {
        private static readonly WeakDictionary<Element,Action<MouseEvent>> _mouseDownListeners = new WeakDictionary<Element, Action<MouseEvent>>();
        private static readonly WeakDictionary<Element,Action<MouseEvent>> _mouseUpListeners = new WeakDictionary<Element, Action<MouseEvent>>();
        private static readonly WeakDictionary<Element,Action<MouseEvent>> _mouseClickListeners = new WeakDictionary<Element, Action<MouseEvent>>();
        private static readonly WeakDictionary<Element,Action<MouseEvent>> _mouseMoveListeners = new WeakDictionary<Element, Action<MouseEvent>>();
        
        public MouseObserver() {
            Document.AddEventListener("mousedown", rawEv => {
                var ev = (MouseEvent)rawEv;

                Document.Body.TraverseAll(el => {
                    if (!el.IsMouseEventRecipient()) {
                        return false;
                    }

                    if (!_mouseDownListeners.ContainsKey(el)) {
                        return false;
                    }
                    Logger.Debug(GetType(), "invoking mousedown listener");
                    _mouseDownListeners.Get(el)(ev);
                    return true;
                });
            });

            Document.AddEventListener("mouseup", rawEv => {
                var ev = (MouseEvent)rawEv;

                Document.Body.TraverseAll(el => {
                    if (!el.IsMouseEventRecipient()) {
                        return false;
                    }

                    if (!_mouseUpListeners.ContainsKey(el)) {
                        return false;
                    }
                    Logger.Debug(GetType(), "invoking mouseup listener");
                    _mouseUpListeners.Get(el)(ev);
                    return true;
                });
            });

            Document.AddEventListener("click", rawEv => {
                var ev = (MouseEvent)rawEv;

                Document.Body.TraverseAll(el => {
                    if (!el.IsMouseEventRecipient()) {
                        return false;
                    }

                    if (!_mouseClickListeners.ContainsKey(el)) {
                        return false;
                    }
                    Logger.Debug(GetType(), "invoking mouseup listener");
                    _mouseClickListeners.Get(el)(ev);
                    return true;
                });
            });
            
            Document.AddEventListener("mousemove", ev => {
                Document.Body.TraverseAll(el => {
                    if (!el.IsMouseEventRecipient()) {
                        return false;
                    }

                    if (!_mouseMoveListeners.ContainsKey(el)) {
                        return false;
                    }

                    //Logger.Debug(GetType(), "invoking mousemove listener");
                    _mouseMoveListeners.Get(el)((MouseEvent)ev);
                    return true;
                }, Magics.PurposeMouseMove);
            });
        }
        
        public void AddMouseClickListener(Element target, Action<MouseEvent> action) {
            target.MarkAsMouseEventRecipient(true); 
            _mouseClickListeners.Set(target, action);
        }
        
        public void AddMouseDownListener(Element target, Action<MouseEvent> action) {
            target.MarkAsMouseEventRecipient(true); 
            _mouseDownListeners.Set(target, action);
        }

        public void AddMouseUpListener(Element target, Action<MouseEvent> action) {
            target.MarkAsMouseEventRecipient(true); 
            _mouseUpListeners.Set(target, action);
        }

        public void AddMouseMoveListener(Element target, Action<MouseEvent> action) {
            target.MarkAsMouseEventRecipient(true); 
            _mouseMoveListeners.Set(target, action);
        }
    }
}
