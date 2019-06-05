using System.Collections.Generic;

namespace Philadelphia.Common {
    public class MenuItemModel {
        public int Id {get; }
        public string DescriptonOrNull;
        public IReadOnlyValue<string> Label;
        public IEnumerable<MenuItemModel> Items {get; }
        public bool IsLeaf => Items == null;
        public bool IsSubTree => !IsLeaf;

        private MenuItemModel(int menuItemId, IReadOnlyValue<string> label, IEnumerable<MenuItemModel> subItemsOrNull, string descriptonOrNull) {
            Id = menuItemId;
            DescriptonOrNull = descriptonOrNull;
            Label = label;
            Items = subItemsOrNull;
        }

        public static MenuItemModel CreateLeaf(int menuItemId, IReadOnlyValue<string> label, string descriptonOrNull = null) {
            return new MenuItemModel(menuItemId, label, null, descriptonOrNull);
        }

        public static MenuItemModel CreateSubTree(IReadOnlyValue<string> label, IEnumerable<MenuItemModel> subItems) {
            return new MenuItemModel(-1, label, subItems, null);
        }
    }
}
