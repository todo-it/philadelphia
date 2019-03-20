using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class SpecificChildListChangedObserver {
        // REVIEW: make field local?
        private readonly MutationObserver _observer;
        private readonly WeakDictionary<Element,HashSet<Action>> _onAddedListeners = new WeakDictionary<Element, HashSet<Action>>(); 

        public SpecificChildListChangedObserver() {
            _observer = new MutationObserver((changes, _) => {
                Logger.Debug(GetType(), "[SpecificChildListChangedObserver] dom nodes mutation detected - got {0} items", changes.Length);
                var triggered = 0;
                changes.ForEach(change =>
                    change.AddedNodes
                        .Where(x => x.NodeType == NodeType.Element)
                        .Select(x => x.AsElementOrNull())
                        .ForEach(added => 
                            triggered += added.TraverseAll(el => {
                                if (!_onAddedListeners.ContainsKey(el)) {
                                    return false;
                                }
                                _onAddedListeners[el].ForEach(x => x());
                                return true;
                            }))
                );
                Logger.Debug(GetType(), "triggered {0} listeners", triggered);
            });

            _observer.Observe(Document.Body, new MutationObserverInit {
                Subtree = true,
                ChildList = true
            });
        }

        public void RegisterListenerOnAdded(Element el, Action action) {
            if (!_onAddedListeners.ContainsKey(el)) {
                _onAddedListeners[el] = new HashSet<Action>();
            } 

            _onAddedListeners[el].Add(action);
        }
    }
}
