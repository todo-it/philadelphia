using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    /// <summary>
    /// An IView containing multiple tabs aligned horizontally where only one tab is visible at the time.
    /// 
    /// Design decision: it doesn't hide inactive tab contents, it detaches them. Why? Because thanks to that one 
    /// can efficiently rely on DOM events to know that element became 'visible'. It is needed f.e. for 
    /// DataGrid column length calculation
    /// </summary>
    public class HorizontalTabbedView : IView<HTMLElement> {
        private readonly HTMLDivElement _container;
        private readonly HTMLDivElement _tabHandleContainer;
        private readonly HTMLDivElement _tabContentContainer;
        private readonly List<HTMLElement> _tabContents = new List<HTMLElement>();
        private int _activeTab = -1;
        private bool _measured;

        public HTMLDivElement TabContentContainer => _tabContentContainer;
        public HTMLElement Widget => _container;

        private HorizontalTabbedView(Action<HorizontalTabbedView> addTabs) {
            _container = new HTMLDivElement {
                Id = UniqueIdGenerator.GenerateAsString(),
                ClassName = GetType().FullName
            };

            _tabHandleContainer = new HTMLDivElement {ClassName = Magics.CssClassTabHandleContainer};

            _tabContentContainer = new HTMLDivElement {ClassName = Magics.CssClassTabContentContainer};

            _container.AppendChild(_tabHandleContainer);
            _container.AppendChild(_tabContentContainer);

            _tabHandleContainer.OnClick += ev => {
                if (!ev.HasHtmlTarget()) {
                    return;
                }

                var target = ev.HtmlTarget();

                if (target == _tabHandleContainer) {
                    return;
                }

                var newActiveTabHandle = target.GetParentElementHavingParent(_tabHandleContainer);
                var newActiveTabIdx = _tabHandleContainer.Children.IndexOfUsingEquals(newActiveTabHandle);
                
                if (_activeTab == newActiveTabIdx) {
                    Logger.Debug(GetType(), "already active tab selected {0}", _activeTab);
                    return;
                }

                Logger.Debug(GetType(), "switching tab from {0} to {1}", _activeTab, newActiveTabIdx);

                if (newActiveTabIdx < 0 || newActiveTabIdx >= _tabContents.Count) {
                    Logger.Error(GetType(), "there's no tab at index {0}. ignoring tab switch", newActiveTabIdx);
                    return;
                }

                if (_activeTab >= 0) {
                    _tabHandleContainer.Children[_activeTab].ClassList.Remove(Magics.CssClassActive);
                    _tabContentContainer.RemoveAllChildren();
                }
                
                ActivateTab(newActiveTabIdx);
            };

            addTabs(this);
            
            DocumentUtil.AddElementAttachedToDocumentListener(_container, () => {
                if (_measured) {
                    return;
                }

                var formerlyFocused = Document.ActiveElement;
                Logger.Debug(GetType(), "Measuring tab heights in onAttached");

                // measure tab content
                var oldVis = _tabContentContainer.Style.Visibility;
                _tabContentContainer.Style.Visibility = Visibility.Hidden;
                
                _tabContents.ForEachI((i,tab) => {
                    _tabContentContainer.RemoveAllChildren();
                    _tabContentContainer.AppendChild(tab);
                });

                _tabContentContainer.RemoveAllChildren();
                _tabContentContainer.Style.Visibility = oldVis;
                _measured = true;
                
                //assure that focused element within tab stays focused(it may have been lost during measurement process above)
                if (_tabContents.Any()) {
                    ActivateTab(_activeTab);
                    if (formerlyFocused != Document.ActiveElement) {
                        formerlyFocused.TryFocusElement();
                    }
                }
            });

            //activate first tab
            if (_tabContents.Any()) {
                ActivateTab(0);
            }
        }
        
        /// <summary> build 'table like' tabbed view with 'errors' indicator in tab handle</summary>
        public static HorizontalTabbedView CreateTableLikeObserved(params Func<ITabContent,string>[] tabs) {
            return new HorizontalTabbedView(self => tabs.ForEach(tabf => {
                Logger.Debug(typeof(HorizontalTabbedView), "tab building start");
                var indicator = new ValidationIndicatorView();
                var forwarder = new ErrorObserver(x => indicator.Value = x <= 0);
                var tabAcc = new TabContentAccumulator(forwarder.Update);

                var tabName = tabf(tabAcc);
                
                var tabData = self.AddTab(cntn => {
                    cntn.ClassList.Add(Magics.CssClassTableLike);
                    tabAcc.Widgets.ForEach(w => cntn.AppendChild(w));
                    return tabName;
                });

                tabData.TabHandle.AppendChild(indicator.Widget);
                Logger.Debug(typeof(HorizontalTabbedView), "tab building finished");
            }));
        }

        /// <summary> build 'table like' tabbed view without providing 'errors' indicator in tab handle</summary>
        /// <param name="tabs"></param>
        public static HorizontalTabbedView CreateTableLikeUnobserved(params Tuple<string,IView<HTMLElement>[]>[] tabs) {
            return new HorizontalTabbedView(self => tabs.ForEach(tab => {
                self.AddTab(cntn => {
                    cntn.ClassList.Add(Magics.CssClassTableLike);
                    tab.Item2.ForEach(w => cntn.AppendChild(w.Widget));
                    return tab.Item1;
                });
            }));
        }
        
        /// <summary> build generic tabbed view without providing 'errors' indicator in tab handle</summary>
        /// <param name="tabs"></param>
        public static HorizontalTabbedView CreateGeneric(params Tuple<string,IView<HTMLElement>[]>[] tabs) {
            return new HorizontalTabbedView(self => tabs.ForEach(tab => {
                self.AddTab(cntn => {
                    tab.Item2.ForEach(w => cntn.AppendChild(w.Widget));
                    return tab.Item1;
                });
            }));
        }
        
        /// <summary> build generic tabbed view without providing 'errors' indicator in tab handle</summary>
        /// <param name="tabs"></param>
        public static HorizontalTabbedView CreateGeneric(params Tuple<string,Action<HTMLElement>>[] tabs) {
            return new HorizontalTabbedView(self => tabs.ForEach(tab => {
                self.AddTab(cntn => {
                    tab.Item2(cntn);
                    return tab.Item1;
                });
            }));
        }

        /// <summary> build custom tabbed view </summary>
        /// <param name="tabs"></param>
        public static HorizontalTabbedView CreateGeneric(params Func<HTMLElement,string>[] tabs) {
            return new HorizontalTabbedView(x => tabs.ForEach(y => x.AddTab(y)));
        }

        private void ActivateTab(int newActiveTabIdx) {
            _activeTab = newActiveTabIdx;

            _tabContentContainer.RemoveAllChildren();
            _tabHandleContainer.Children[_activeTab].ClassList.Add(Magics.CssClassActive);
            var activeTabContent = _tabContents[_activeTab];
            _tabContentContainer.AppendChild(activeTabContent);
        }

        private TabData AddTab(Func<HTMLElement,string> contentAdder) {
            var tab = CreateTab();
            
            _tabHandleContainer.AppendChild(tab.TabHandle);
            
            tab.SetLabel(
                contentAdder(tab.TabContent));
            _tabContents.Add(tab.TabContent);

            return tab;
        }

        private TabData CreateTab() {
            var tabHandle = new HTMLDivElement {ClassName = Magics.CssClassTabHandle};
            var tabLabel = new HTMLDivElement {ClassName = Magics.CssClassTabLabel};
            tabHandle.AppendChild(tabLabel);

            var tabContent = new HTMLDivElement {ClassName = Magics.CssClassTabContent};

            return new TabData {
                TabHandle = tabHandle,
                TabLabel = tabLabel,
                TabContent = tabContent
            };
        }

        public static implicit operator RenderElem<HTMLElement>(HorizontalTabbedView inp) {
            return RenderElem<HTMLElement>.Create(inp);
        }
    }
}
