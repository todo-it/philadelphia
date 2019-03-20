using System;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class SpecificResizeObserver {
        private static readonly WeakDictionary<Element,Action> _listeners = new WeakDictionary<Element, Action>(); 
        
        public SpecificResizeObserver() {
            Window.AddEventListener(
                "resize", 
                () => {
                    Logger.Debug(GetType(), "[SpecificResizeObserver] updating listeners?");
                    var updated = Document.Body.TraverseAll(x => {
                        if (!x.IsResizeRecipient() || !_listeners.ContainsKey(x)) {
                            return false;
                        }
                        _listeners[x]();
                        return true;
                    });

                    Logger.Debug(GetType(), "updated {0} listeners", updated);
                }
            );
        }

        public void RegisterListener(Element el, Action action) {
            el.MarkAsResizeRecipient(true);
            _listeners[el] = action;
        }
    }
}
