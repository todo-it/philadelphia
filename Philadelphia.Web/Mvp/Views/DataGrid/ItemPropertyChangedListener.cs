using System;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class ItemPropertyChangedListener<T> : ITypedSubscribeable<T> where T:new() {
        private readonly Func<T, ISubscribeable> _asSubscribeable;
        private readonly string _propertyName;

        public ItemPropertyChangedListener(Func<T,ISubscribeable> asSubscribeable, Func<T,string> propertyNameProvider) {
            _asSubscribeable = asSubscribeable;
            _propertyName = propertyNameProvider(new T());
        }

        public void Subscribe(T forRecord, Action listener) {
            _asSubscribeable(forRecord).Subscribe(_propertyName, listener);
        }

        public void Unsubscribe(T forRecord, Action listener) {
            _asSubscribeable(forRecord).Unsubscribe(_propertyName, listener);
        }
    }
}
