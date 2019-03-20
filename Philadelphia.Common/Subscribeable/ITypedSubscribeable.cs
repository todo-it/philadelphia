using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphia.Common {
    public interface ITypedSubscribeable<in T> {
        void Subscribe(T trait, Action listener);
        void Unsubscribe(T trait, Action listener);
    }
}
