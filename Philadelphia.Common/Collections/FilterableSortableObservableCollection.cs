using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Philadelphia.Common {
    public class FilterableSortableObservableCollection<T> : IFilterableCollection<T>,ISortableCollection<T> {
        private List<T> _allItems = new List<T>();
        private readonly List<T> _visibleItems = new List<T>();
        private readonly List<Action<IWhereCollector<T>>> _latestFilters = new List<Action<IWhereCollector<T>>>();
        private readonly IDictionary<T,int> _visibleItemsIdx = new Dictionary<T, int>();

        public event CollectionChanged<T> Changed;
		
        public IEnumerable<T> Items => _visibleItems;

        public int Length => _visibleItems.Count;

        public T this[int position] => _visibleItems[position];

        public FilterableSortableObservableCollection(IEnumerable<T> initialItems) {
            _allItems.AddRange(initialItems);
            _visibleItems.AddRange(_allItems);
            RebuildIdx();
        }

        private void RebuildIdx() {
            _visibleItemsIdx.Clear();
            _visibleItems.ForEachI((i,x) => _visibleItemsIdx.Add(x, i));
        }

        public FilterableSortableObservableCollection() : this(new List<T>()) {}
        
        public int IndexOf(T inp) => _visibleItemsIdx.TryGetValue(inp, out var res) ? res : -1;

        private void RememberFilter(Action<IWhereCollector<T>> filterFunction, int at) {
            //assure index is within table
            while (at+1 > _latestFilters.Count) {
                _latestFilters.Add(null);
            }
            _latestFilters[at] = filterFunction;
        }

        public void Clear() {
            Logger.Debug(GetType(),"clear()");
            var deletedCopy = _visibleItems.ToArray();

            _visibleItems.Clear();
            _allItems.Clear();
            RebuildIdx();

            Changed?.Invoke(-1, new T[0], deletedCopy);
        }

        public void DeleteAt(int position) {
            Logger.Debug(GetType(),"deleteAt({0})", position);
            var deletedItem = _visibleItems[position];
            
            _visibleItems.RemoveAt(position);
            _allItems.Remove(deletedItem);

            RebuildIdx(); //slow

            Changed?.Invoke(-1, new T[0], new [] {deletedItem});
        }

        public void Delete(params T[] items) {
            Logger.Debug(GetType(),"delete(count={0})", items.Length);

            var actuallyRemoved = new List<T>();

            foreach (var item in items) {
                _visibleItems.Remove(item);
                var reallyRemoved = _allItems.Remove(item);
                
                if (reallyRemoved) {
                    actuallyRemoved.Add(item);
                }
            }
            RebuildIdx();

            Changed?.Invoke(-1, new T[0], actuallyRemoved.ToArray());
        }

        public void InsertAt(int position, params T[] items) {
            Logger.Debug(GetType(),"ObservableCollection->insertAt({0}, {1})", position, items);
            
            _visibleItems.InsertRange(position, items);
            _allItems.AddRange(items);
            
            RebuildIdx();

            Changed?.Invoke(position, items, new T[0]);
        }

        public void ChangeItemsSorting(Action<IOrderByCollector<T>> collect) {
            var orderingCollector = new PassToLinqOrderByCollector<T>(_allItems);
            collect(orderingCollector);
            
            var newAllItems = orderingCollector.GetSortedResult().ToList();
            T[] newVisibleItems;

            if (!_latestFilters.Any() || _latestFilters.All(x => x == null)) {
                newVisibleItems = newAllItems.ToArray();
            } else {
                var filteringCollector = new PassToLinqWhereCollector<T>(newAllItems);
                _latestFilters.Where(x => x != null).ForEach(x => x(filteringCollector));
                newVisibleItems = filteringCollector.GetFilteredResult().ToArray();
            }
            
            //simulate that all visible-till-now items are removed...
            var oldVisibleItems = _visibleItems.ToArray();
            Logger.Debug(GetType(), "Removing all rows as order change will likely demand it");
            
            _visibleItems.Clear();
            
            //... and simulate that visible set is added now
            _allItems = newAllItems;
            _visibleItems.AddRange(newVisibleItems);
            RebuildIdx();

            Logger.Debug(GetType(), "Filtering caused that {0} rows are visible now", newVisibleItems.Length);
            
            Changed?.Invoke(0, newVisibleItems, oldVisibleItems);
        }

        public void ChangeItemsFilter(Action<IWhereCollector<T>> filterFunction, int filterApplyOrder) {
            RememberFilter(filterFunction, filterApplyOrder);
            var collector = new PassToLinqWhereCollector<T>(_allItems);
            _latestFilters.Where(x => x!= null).ForEach(x => x(collector));

            var newVisibleItems = collector.GetFilteredResult().ToArray();

            //simulate that all visible-till-now items are removed...
            var oldVisibleItems = _visibleItems.ToArray();
            Logger.Debug(GetType(), "Removing all rows as filter changed will likely demand it. {0} items", oldVisibleItems.Length);

            _visibleItems.Clear();
            
            //... and simulate that visible set is added now
            _visibleItems.AddRange(newVisibleItems);
            RebuildIdx();
            Logger.Debug(GetType(), "Filtering caused that {0} rows are visible now", newVisibleItems.Length);
            
            Changed?.Invoke(0, newVisibleItems, oldVisibleItems);
        }

        public IEnumerator<T> GetEnumerator() => _visibleItems.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _visibleItems.GetEnumerator();

        public void Replace(params T[] newItems) {
            var deletedCopy = _visibleItems.ToArray();
            Logger.Debug(GetType(),"replace() existing={0} newItems={1}", deletedCopy.Length, newItems.Length);
			
            _visibleItems.Clear();
            _allItems.Clear();
            
            _visibleItems.InsertRange(0, newItems);
            _allItems.AddRange(newItems);
            RebuildIdx();

            Changed?.Invoke(0, newItems, deletedCopy);
        }
    }
}
