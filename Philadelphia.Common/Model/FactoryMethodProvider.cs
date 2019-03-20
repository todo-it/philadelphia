using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphia.Common {
    public class FactoryMethodProvider<T> : IProvider<T> {
        private readonly Func<T> _provide;

        public FactoryMethodProvider(Func<T> provide) {
            _provide = provide;
        }

        public T Provide() {
            return _provide();
        }
    }
}
