using System;
using System.Collections.Generic;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public interface IMenuBarView : IView<HTMLElement> {
        event Action<int> ItemActivated;
        IEnumerable<MenuItemModel> Items {set; }
        IEnumerable<IView<HTMLElement>> Actions {get; }
    }
}
