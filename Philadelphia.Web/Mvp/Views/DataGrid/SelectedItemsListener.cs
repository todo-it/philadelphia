using System;
using System.Collections.Generic;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class SelectedItemsListener<T> : ITypedSubscribeable<T> {
        private readonly Func<DataGridModel<T>> _dgmodel;
        private readonly Dictionary<T,CollectionChanged<T>> _listeners = new Dictionary<T, CollectionChanged<T>>();

        public SelectedItemsListener(Func<DataGridModel<T>> dgmodel) {
            _dgmodel = dgmodel;
        }

        public void Subscribe(T forRecord, Action listener) {
            //create one function instance so that it can be unsubscribed later
            void Handler(int at, T[] inserted, T[] removed) => listener();
            _dgmodel().Selected.Changed += Handler;
            _listeners[forRecord] = Handler;
        }

        public void Unsubscribe(T forRecord, Action listener) {
            var handler = _listeners[forRecord];
            _dgmodel().Selected.Changed -= handler;
            _listeners.Remove(forRecord);
        }
    }
}
