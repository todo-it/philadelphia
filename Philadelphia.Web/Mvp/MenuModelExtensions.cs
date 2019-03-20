using Philadelphia.Common;

namespace Philadelphia.Web {
    public static class MenuModelExtensions {
        public static void Bind(this MenuModel self, IMenuBarView view) {
            view.ItemActivated += async x => await self.ItemActivated(x);
            self.ItemsChanged += _ => view.Items = self.Items;
        }

        public static void BindAndInitialize(this MenuModel self, IMenuBarView view) {
            self.Bind(view);
            view.Items = self.Items;
        }
    }
}
