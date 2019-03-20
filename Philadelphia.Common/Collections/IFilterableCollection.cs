using System;

namespace Philadelphia.Common {
    public interface IFilterableCollection<T> : IObservableCollection<T> {
        /// <summary>
        /// applies filter to collection. Collection remembers filterFunctions in collection. 
        /// filterApplyOrder is an index of that colelction. Filters are applied in order of filterApplyOrder
        /// </summary>
        /// <param name="filterFunction"></param>
        /// <param name="filterApplyOrder">must be zero or bigger</param>
        void ChangeItemsFilter(Action<IWhereCollector<T>> filterFunction, int filterApplyOrder);
    }
}
