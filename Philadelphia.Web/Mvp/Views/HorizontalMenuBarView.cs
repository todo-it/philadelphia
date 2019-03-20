using System;
using System.Collections.Generic;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class HorizontalMenuBarView : IMenuBarView {
        private readonly HTMLElement _nav;
        private readonly HTMLElement _root;
        private readonly Action<Event> _onItemClicked;
        private readonly Action<Event> _onMouseOver;

        public event Action<int> ItemActivated;

        public IEnumerable<IView<HTMLElement>> Actions {
            get {
                //TODO implement it recursively
                throw new NotImplementedException();
            }
        }

        public HorizontalMenuBarView() {
            _nav = DocumentUtil.CreateElementHavingClassName("nav", GetType().FullName);
            _nav.Id = UniqueIdGenerator.GenerateAsString();

            _root = new HTMLElement("ul") {Id = UniqueIdGenerator.GenerateAsString()};
            _nav.AppendChild(_root);
            
            _onItemClicked = ev => {
                if (!ev.HasHtmlTarget()) {
                    return;
                }

                ev.PreventDefault();

                var htmlTarget = ev.HtmlTarget();
                var menuItemId = htmlTarget.GetAttribute(Magics.AttrDataMenuItemId);
                Logger.Debug(GetType(),"user activated menuItemId {0} in view", menuItemId);
                ActivateAllBut(_root, new List<HTMLElement>());
                ItemActivated?.Invoke(Convert.ToInt32(menuItemId));
            };
            _onMouseOver = ev => {               
                if (!ev.HasHtmlCurrentTarget()) {
                    return;
                }

                var hoverOn = ev.HtmlCurrentTarget();
                var active = new List<HTMLElement> {hoverOn};

                var parent = hoverOn.ParentElement;
                while (parent != _root) {
                    active.Add(parent);
                    parent = parent.ParentElement;
                }

                ActivateAllBut(_root, active);
                hoverOn.Focus();
            };
            
            Document.Body.AddEventListener("click", ev => {
                //find out if clicked item is a descendant of menu - if not fold whole menu
                
                if (!ev.HasHtmlTarget()) {
                    return;
                }

                if (ev.HtmlTarget().IsDescendantOf(_nav)) {
                    return;
                }

                ActivateAllBut(_root, new List<HTMLElement>());
            });
        }

        public void DecorateAsFormView() {
            _nav.MarkAsFormView(false);
            _root.ClassList.Add(Magics.CssClassBody);
        }

        public HTMLElement Widget => _nav;

        public IEnumerable<MenuItemModel> Items {
            set {
                RemoveItems(_root);
                AddItems(_root, value);
            }
        }
        
        private void RemoveItems(Element from) {
            from.RemoveAllChildren(); //nothing fancy as GC should cleanup whole mess...
        }

        private void SetupSubMenuPopup(Element hoverOn, Element whatToShow, bool isMenu) {
            hoverOn.AddEventListener("mouseover", _onMouseOver);
        }

        private void ActivateAllBut(HTMLElement within, List<HTMLElement> toBeActivated) {
            var active = toBeActivated.Contains(within);
            
            within.AddOrRemoveClass(active, Magics.CssClassActive);
            within.AddOrRemoveClass(!active, Magics.CssClassInactive);
            
            foreach (var el in within.ChildElements()) {
                ActivateAllBut(el, toBeActivated);
            }
        }
        
        private void AddItems(Element into, IEnumerable<MenuItemModel> toAdd, int nestingLevel = 0) {
            foreach (var itemToAdd in toAdd) {
                var chld = new Element("li") {ClassName = Magics.CssClassInactive};
                chld.SetAttribute("tabindex", "0");

                into.AppendChild(chld);

                var anchor = new HTMLAnchorElement {
                    Href = "#",
                    TextContent = itemToAdd.Label
                };

                chld.AppendChild(anchor);

                if (itemToAdd.IsLeaf) {    
                    anchor.SetAttribute(Magics.AttrDataMenuItemId, itemToAdd.Id.ToString());
                    anchor.AddEventListener("click", _onItemClicked);
                    SetupSubMenuPopup(chld, chld.FirstElementChild, false);
                } else {
                    var ul = new Element("ul");
                    anchor.SetAttribute("onclick", "return false;");
                    chld.AppendChild(ul);
                    AddItems(ul, itemToAdd.Items, nestingLevel+1);
                    SetupSubMenuPopup(chld, chld.FirstElementChild, true);
                }
            }
        }
        
        public static implicit operator RenderElem<HTMLElement>(HorizontalMenuBarView inp) {
            return RenderElem<HTMLElement>.Create(inp);
        }
    }
}
