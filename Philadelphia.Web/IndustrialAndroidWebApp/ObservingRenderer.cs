using System;
using System.Linq;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public delegate void OnFormRendererEvent(IFormRenderer<HTMLElement> sender, IBareForm<HTMLElement> added, IBareForm<HTMLElement> removed);

    public class ObservingRenderer : IFormRenderer<HTMLElement> {
        private readonly IFormRenderer<HTMLElement> _adapted;
        private readonly OnFormRendererEvent _callback;

        public IBareForm<HTMLElement> Master => _adapted.Master;
        public IBareForm<HTMLElement> TopMostPopup => _adapted.TopMostPopup;

        public ObservingRenderer(IFormRenderer<HTMLElement> adapted, OnFormRendererEvent callback) {
            _adapted = adapted;
            _callback = callback;
        }

        public void ReplaceMasterWithAdapter(IView<HTMLElement> view) =>
            _adapted.ReplaceMasterWithAdapter(view);
        
        public void ReplaceMaster(IBareForm<HTMLElement> newForm) {
            var maybeMaster = _adapted.Master;
            _adapted.ReplaceMaster(newForm);
            _callback(this, newForm, maybeMaster);
        }

        public void AddPopup(IBareForm<HTMLElement> newForm) {
            _adapted.AddPopup(newForm);
            _callback(this, newForm, null);
        }

        public void Remove(IBareForm<HTMLElement> frm) {
            _adapted.Remove(frm);
            _callback(this, null, frm);
        }

        public void ClearMaster() {
            var maybeMaster = _adapted.Master;
            _adapted.ClearMaster();
            _callback(this, null, maybeMaster);
        }

        public IFormRenderer<HTMLElement> CreateRendererWithBase(IFormCanvas<HTMLElement> masterCanvas) =>
            _adapted.CreateRendererWithBase(masterCanvas);
    }
    
    public static class FormRendererExtensions {
        public static IFormRenderer<HTMLElement> Observe(this IFormRenderer<HTMLElement> self, OnFormRendererEvent callback) =>
            new ObservingRenderer(self, callback);
        
        private static void OnFormRendererEvent(
            IFormRenderer<HTMLElement> sender, ToolbarSettings toolbarSettings) {
            
            var maybeTopMostPopup = sender.TopMostPopup;
            Logger.Debug(typeof(FormRendererExtensions), "OnFormRendererEvent master={0} popup={1}", sender.Master, maybeTopMostPopup);
            
            if (maybeTopMostPopup is IAugmentsToolbar) {
                ((IAugmentsToolbar)sender.TopMostPopup).OnAugmentToolbar(toolbarSettings);
            } else if (sender.Master is IAugmentsToolbar) {
                ((IAugmentsToolbar)sender.Master).OnAugmentToolbar(toolbarSettings);
            }
            
            //TODO optimization opportunity: store "former settings" and compare it against new ones to execute necessary calls only
            IawAppApi.SetToolbarColors(toolbarSettings.AppBarBackgroundColor, toolbarSettings.AppBarForegroundColor);
            IawAppApi.SetToolbarSearchState(toolbarSettings.SearchCallback);
            IawAppApi.SetToolbarItems(activated => {
                    Logger.Debug(typeof(FormRendererExtensions), "OnMenuItemActivated raw menuItem {0}", activated);
                    var maybeMenuItem = toolbarSettings.MenuItems.FirstOrDefault(x => x.Item1.webMenuItemId == activated.webMenuItemId);
                    Logger.Debug(typeof(FormRendererExtensions), "OnMenuItemActivated found?={0}", maybeMenuItem != null);
                    maybeMenuItem?.Item2();
                }, 
                toolbarSettings.MenuItems.Select(x => x.Item1));
            IawAppApi.SetToolbarBackButtonState(toolbarSettings.BackActionVisible);
        }

        public static IFormRenderer<HTMLElement> Observe(this IFormRenderer<HTMLElement> self, Func<IFormRenderer<HTMLElement>,ToolbarSettings> callback) =>
            new ObservingRenderer(self, (sender, _, __) => OnFormRendererEvent(sender, callback(sender)));
    }
}
