using System;
using System.Collections.Generic;

namespace Philadelphia.Web {
    public class IterableIEnumerator<T> : IEnumerator<T> {
        private readonly IIterable<T> _src;
        private T _current;
        public object Current => _current;
        T IEnumerator<T>.Current => _current;

        public IterableIEnumerator(IIterable<T> src) {
            _src = src;
        }

        public bool MoveNext() {
            var r = _src.next();
            _current = r.value;

            return !r.done;
        }

        public void Reset() {
            throw new NotSupportedException();
        }
        
        public void Dispose() {}
    }
}
