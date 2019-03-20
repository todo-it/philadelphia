using System;
using System.Collections.Generic;
using System.Linq;
using Bridge;
using Bridge.Html5;

namespace Philadelphia.Web {
    /// <summary>
    /// Thin wrapper around <see cref="WeakMap"/>
    /// see also https://github.com/bridgedotnet/Bridge/pull/1444
    /// </summary>
    public class WeakDictionary<TKey,TValue> where TKey : class {
        private WeakMap _impl;

        public WeakDictionary() {
            _impl = new WeakMap();
        }
        
        public WeakDictionary(IEnumerable<KeyValuePair<TKey,TValue>> items) {
            _impl = new WeakMap(
                items.Select(x => new object[] {x.Key, x.Value}).ToArray()
            );
        }

        /// <summary>
        /// Clear all elements from Dictionary
        /// </summary>
        public void Clear() {
            //as WeakMap.clear() is not widely implemented it uses workaround
            _impl = new WeakMap();
        }

        public TValue this[TKey key] {
            get {
                return Get(key);
            }
            set {
                Set(key, value);
            }
        }

        public TValue Get(TKey key) {
            var result = _impl.Get(key);
            if (result == null) {
                // friendlier exception instead of default Bridge.InvalidCastException
                throw new KeyNotFoundException("No value for key present in WeakMap");
            }
            
            return BridgeObjectUtil.NoOpCast<TValue>(result);
        }

        public void Set(TKey key, TValue value) {
            _impl.Set(key, value);
        }

        public bool Delete(TKey key) {
            return _impl.Delete(key);
        }

        public bool ContainsKey(TKey key) {
            return _impl.Has(key);
        }
    }
}
