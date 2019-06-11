using System.Collections.Generic;

namespace Philadelphia.Common {
    public class MenuItemModel {
        public int Id {get; }
        public string DescriptonOrNull;
        public IReadOnlyValue<string> Label;
        public IEnumerable<MenuItemModel> Items {get; }
        public bool IsLeaf => Items == null;
        public bool IsSubTree => !IsLeaf;
        public MenuItemUserModel Source { get; }

        private MenuItemModel(
                int menuItemId, IReadOnlyValue<string> label, IEnumerable<MenuItemModel> subItemsOrNull, 
                string descriptionOrNull, MenuItemUserModel source) {

            Id = menuItemId;
            DescriptonOrNull = descriptionOrNull;
            Label = label;
            Items = subItemsOrNull;
            Source = source;
        }

        public static MenuItemModel CreateLeaf(
                int menuItemId, IReadOnlyValue<string> label, MenuItemUserModel source,
                string descriptionOrNull = null) {

            return new MenuItemModel(menuItemId, label, null, descriptionOrNull, source);
        }

        public static MenuItemModel CreateSubTree(
                IReadOnlyValue<string> label, MenuItemUserModel source,
                IEnumerable<MenuItemModel> subItems) {

            return new MenuItemModel(-1, label, subItems, null, source);
        }
    }
}
