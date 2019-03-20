using System;
using System.Collections.Generic;
using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Web;
using System.Linq;

namespace Philadelphia.Web {
    public class ActionButtonsMenuBarView : IMenuBarView {
        private readonly Func<MenuItemModel,InputTypeButtonActionView> _buttonBuilder;
        private readonly HTMLElement _nav;
        private readonly HTMLDivElement _actionsCntnr;
        private readonly List<InputTypeButtonActionView> _items = new List<InputTypeButtonActionView>();
        
        public HTMLElement Widget => _nav;
        public event Action<int> ItemActivated;
        public IEnumerable<IView<HTMLElement>> Actions => _items;

        public IEnumerable<MenuItemModel> Items {
            set {
                _actionsCntnr.RemoveAllChildren();
                _items.Clear();

                foreach (var itemToAdd in value) {
                    if (itemToAdd.IsSubTree) {
                        throw new Exception("doesn't support folders yet");
                    }
                    
                    var btn = _buttonBuilder(itemToAdd);
                    
                    btn.Triggered += () => {
                        DeactivateAllExceptFor(btn);
                        ItemActivated?.Invoke(itemToAdd.Id);
                    };
                    _actionsCntnr.AppendChild(btn.Widget);
                    _items.Add(btn);
                }
            }
        }
        
        private void DeactivateAllExceptFor(InputTypeButtonActionView btn) {
            _items.ForEach(x => x.IsPressed = false);
            btn.IsPressed = true;
        }

        public ActionButtonsMenuBarView(
                Func<MenuItemModel,InputTypeButtonActionView> customButtonBuilder = null) {

            _buttonBuilder = customButtonBuilder ?? (x => {
                var res = new InputTypeButtonActionView(x.Label);
                res.Widget.SetAttribute(Magics.AttrDataMenuItemId, x.Id.ToString());
                    
                if (x.DescriptonOrNull != null) {
                    res.Widget.Title = x.DescriptonOrNull;
                }

                return res;
            });

            _nav = DocumentUtil.CreateElementHavingClassName("nav", GetType().FullName);
            _nav.Id = UniqueIdGenerator.GenerateAsString();

            _actionsCntnr = new HTMLDivElement();
            _nav.AppendChild(_actionsCntnr);
        }
        
        public void DecorateAsFormView() {
            _nav.MarkAsFormView(false);
            _actionsCntnr.ClassList.Add(Magics.CssClassBody);
        }

        public static implicit operator RenderElem<HTMLElement>(ActionButtonsMenuBarView inp) {
            return RenderElem<HTMLElement>.Create(inp);
        }
    }
}
