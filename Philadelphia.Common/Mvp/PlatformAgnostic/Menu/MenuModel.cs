using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Philadelphia.Common {
    public class MenuModel {
        private readonly Dictionary<int,IActionModel<Unit>> _idToAction = new Dictionary<int, IActionModel<Unit>>();
        private readonly List<MenuItemModel> _items = new List<MenuItemModel>();
        public event Action<IEnumerable<MenuItemUserModel>> ItemsChanged;

        public IEnumerable<MenuItemModel> Items => _items;

        public MenuModel(IEnumerable<MenuItemUserModel> userItems) {
            RegisterModel(userItems, _items);
        }

        public MenuModel() : this(new List<MenuItemUserModel>()) {}
        
        public void ReplaceItems(params MenuItemUserModel[] userItems) {
            ReplaceItems(userItems.AsEnumerable());
        }

        public void ReplaceItems(IEnumerable<MenuItemUserModel> userItems) {
            var newItems = userItems.ToList();

            _items.Clear();
            RegisterModel(newItems, _items);
            ItemsChanged?.Invoke(newItems);
        }

        private void RegisterModel(IEnumerable<MenuItemUserModel> fromItems, List<MenuItemModel> toItems) {
            foreach (var item in fromItems) {
                if (item.IsLeaf) {
                    var id = UniqueIdGenerator.Generate();
                    _idToAction[id] = item.Action;
                    toItems.Add(MenuItemModel.CreateLeaf(id, item.Label, item.DescriptionOrNull));
                } else {
                    var subItems = new List<MenuItemModel>();
                    RegisterModel(item.Items, subItems);
                    toItems.Add(MenuItemModel.CreateSubTree(item.Label, subItems));
                }
            }
        }

        public Task<Unit> ItemActivated(int itemId) {
            if (!_idToAction.ContainsKey(itemId)) {
                Logger.Debug(GetType(),"Cannot invoke menuitem by id {0} as one doesn't exist", itemId);
                return Task.FromResult(Unit.Instance);
            }

            Logger.Debug(GetType(),"Invoking menuitem by id {0}", itemId);
            return _idToAction[itemId].Trigger();
        }
    }
}
