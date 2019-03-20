using System;

namespace Philadelphia.Common {
    public interface ISortableCollection<T> : IObservableCollection<T> {
        void ChangeItemsSorting(Action<IOrderByCollector<T>> collect);
    }
}
