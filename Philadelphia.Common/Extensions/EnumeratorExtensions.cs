using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphia.Common {
    public class GenericEnumerable<T> : IEnumerable<T> {
        private readonly IEnumerator<T> _enumerator;

        public GenericEnumerable(IEnumerator<T> enumerator) {
            _enumerator = enumerator;
        }

        public IEnumerator<T> GetEnumerator() {
            return _enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            throw new Exception("nongeneric GetEnumerator() is not supported");
        }
    }

    public static class EnumeratorExtensions {
        public static IEnumerable<T> AsEnumerable<T>(this IEnumerator<T> self) {
            return new GenericEnumerable<T>(self);
        }
    }
}
