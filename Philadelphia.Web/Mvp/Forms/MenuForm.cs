using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class MenuForm : IForm<HTMLElement,MenuForm, Unit> {
        public event Action<MenuForm,Unit> Ended;

        // REVIEW: follow R# advice and make Menu prop an autoprop
        private readonly MenuModel _menuModel;

        public MenuModel Menu => _menuModel;
        public string Title { get; set; } = ""; //untitled by default
    
        public IFormView<HTMLElement> View { get; }

        public MenuForm(IEnumerable<MenuItemUserModel> menuItemsRaw) : this(new HorizontalLinksMenuFormView(), menuItemsRaw) {}

        public MenuForm(IMenuFormView view, IEnumerable<MenuItemUserModel> menuItemsRaw) {
            View = view;
            
            _menuModel = new MenuModel();
            _menuModel.ItemsChanged += menuItems => 
                menuItems
                    .Where(x => x.IsExit)
                    .ForEach(x => x.Action.ActionExecuted += _ => Ended?.Invoke(this, Unit.Instance));
            _menuModel.BindAndInitialize(view.MenuBar);

            _menuModel.ReplaceItems(menuItemsRaw.ToList());
        }
        
        public ExternalEventsHandlers ExternalEventsHandlers => ExternalEventsHandlers.Ignore;
    }
}
