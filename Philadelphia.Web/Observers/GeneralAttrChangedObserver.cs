using System;
using System.Collections.Generic;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    /// <summary>
    /// per-attribute-name listeners
    /// </summary>
    public class GeneralAttrChangedObserver {
        private readonly Dictionary<string,Action<HTMLElement>> _listeners = new Dictionary<string, Action<HTMLElement>>();

        // REVIEW: make field local?
        private readonly MutationObserver _observer;
        
        public GeneralAttrChangedObserver() {
            _observer = new MutationObserver((changes, _) => {
                Logger.Debug(GetType(), "[GeneralAttrChangedObserver] dom attr mutation detected - got {0} items", changes.Length);
                var triggered = 0;
                changes.ForEach(change => {
                    var attrName = change.AttributeName.ToLower();
                    var el = change.Target.AsElementOrNull();
                    Logger.Debug(GetType(), "changed {0} to '{1}' on {2}", attrName, el?.GetAttribute(attrName), el);
                    if (!_listeners.ContainsKey(attrName) || el == null) {
                        return;
                    }
                    triggered++;
                    _listeners[attrName](el);
                });
                Logger.Debug(GetType(), "triggered {0} listeners", triggered);
            });

            _observer.Observe(Document.Body, new MutationObserverInit {
                Attributes = true,
                Subtree = true
            });
        }

        public void RegisterListener(string attributeName, Action<HTMLElement> action) {
            _listeners[attributeName.ToLower()] = action;
        }
    }
}
