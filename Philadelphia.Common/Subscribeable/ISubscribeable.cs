using System;

namespace Philadelphia.Common {
    public interface ISubscribeable {
        void Subscribe(string propertyName, Action listener);
        void Unsubscribe(string propertyName, Action listener);
    }
}
