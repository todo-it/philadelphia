using System;
using System.Collections.Generic;

namespace Philadelphia.Common {
    public class Subscribeable : ISubscribeable {
        private IDictionary<string,List<Action>> _listeners;
        
        public void Notify(string propertyName) {
            if (_listeners == null) {
                return;
            }

            if (!_listeners.TryGetValue(propertyName, out var propListeners)) {
                return;
            }
            propListeners.ForEach(x => x());
        }
        
        public void Unsubscribe(string propertyName, Action listener) {
            //assumption - nobody will attempt to unsubscribe from item that was never subscribed to
            //trades efficiency for impossible exception in properly written programs
            // REVIEW: interesting, although I would still add null check here... 
            // question: also maybe we could use IDisposable as return value to subscribe? How would it affect performance?
            // _listeners?[propertyName].Remove(listener); 
            _listeners[propertyName].Remove(listener); 
        }
        
        public void Subscribe(string propertyName, Action listener) {
            if (_listeners == null) {
                _listeners = new Dictionary<string, List<Action>>();
            }

            if (!_listeners.TryGetValue(propertyName, out var propListeners)) {
                propListeners = new List<Action>();
                _listeners[propertyName] = propListeners;
            }
            propListeners.Add(listener);
        }
    }
}
