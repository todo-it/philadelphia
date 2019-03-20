using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class GeneralChildListChangedObserver {
        // REVIEW: make field local?
        private readonly MutationObserver _observer;
        private readonly HashSet<Action<HTMLElement>> _onAddedListeners = new HashSet<Action<HTMLElement>>(); 
        
        public GeneralChildListChangedObserver() {
            _observer = new MutationObserver((changes, _) => {
                Logger.Debug(GetType(), "[GeneralChildListChangedObserver] dom nodes mutation detected - got {0} items", changes.Length);
                var triggered = 0;
                changes.ForEach(change =>
                    change.AddedNodes
                        .Where(x => x.NodeType == NodeType.Element)
                        .Select(x => x.AsElementOrNull())
                        .ForEach(added => 
                            triggered += added.TraverseAll(el => {
                                _onAddedListeners.ForEach(listener => listener(el));
                                return true;
                            }))
                );
                Logger.Debug(GetType(), "triggered {0} listeners {1} times total", _onAddedListeners.Count, triggered);
            });

            _observer.Observe(Document.Body, new MutationObserverInit {
                Subtree = true,
                ChildList = true
            });
        }
        
        public void RegisterListenerOnAdded(Action<HTMLElement> action) {
            _onAddedListeners.Add(action);
        }
    }
}
