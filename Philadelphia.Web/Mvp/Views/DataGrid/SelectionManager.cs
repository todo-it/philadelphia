using System.Linq;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class SelectionManager<T> {
        private readonly IObservableCollection<T> _selectedItems;
        private readonly IObservableCollection<T> _allItems;

        public SelectionManager(IObservableCollection<T> selectedItems, IObservableCollection<T> allItems) {
            _selectedItems = selectedItems;
            _allItems = allItems;
        }

        public void ReplaceWithRange(T from, T to) {
            var iFrom = _allItems.IndexOf(from);
            var iTo = _allItems.IndexOf(to);

            if (iFrom < 0 || iTo < 0) {
                return; //normally unreachable code
            }
            
            if (iFrom>iTo) {
                var tmp = iFrom;
                iFrom = iTo;
                iTo = tmp;
            }
            _selectedItems.Replace(
                _allItems.Skip(iFrom).Take(iTo - iFrom + 1));
        }

        public void ReplaceWithItem(T item) {
            _selectedItems.Replace(item);
        }

        public void Toggle(T item) {
            var wasSelected = _selectedItems.Contains(item);

            if (wasSelected) {
                _selectedItems.Delete(item);
            } else {
                _selectedItems.InsertAt(0, item);
            }
        }
        
        public void OnItemsRemovedFromModel(T[] records) {
            Logger.Debug(GetType(), "OnItemsRemovedFromModel(count={0})", records.Length);
            _selectedItems.Delete(records); 
        }

        public bool Any() {
            return _selectedItems.Any();
        }
    }
}
