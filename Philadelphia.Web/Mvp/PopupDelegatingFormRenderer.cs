using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class PopupDelegatingFormRenderer : IFormRenderer<HTMLElement> {
        private readonly IFormRenderer<HTMLElement> _parent;
        private IView<HTMLElement> _masterView;
        private IFormView<HTMLElement> _currentMaster;
        private readonly IFormCanvas<HTMLElement> _masterCanvas;
        
        public PopupDelegatingFormRenderer(
            IFormRenderer<HTMLElement> parent, IFormCanvas<HTMLElement> masterCanvas) {

            _parent = parent;
            _masterCanvas = masterCanvas;
        }

        public void ClearMaster() {
            if (_masterView != null) {
                Logger.Debug(typeof(FormCanvasExtensions), "Cleaning IView from masterCanvas");
                _masterCanvas.Hide();
                _masterView = null;
            }

            if (_currentMaster == null) {
                return;
            }

            Logger.Debug(GetType(),"Removing master {0}", _currentMaster);
            _masterCanvas.Unrender();
            _currentMaster = null;
        }

        public void ReplaceMasterWithAdapter(IView<HTMLElement> newView) {
            Logger.Debug(GetType(),"Replacing master with view {0}", newView);
            ClearMaster();
            _masterView = newView;
            _masterCanvas.RenderAdapter(newView);
        }

        public void ReplaceMaster(IBareForm<HTMLElement> newForm) {
            var isSame = newForm.View == _currentMaster;
            Logger.Debug(GetType(),"Replacing master with form {0} isSame?={1}", newForm, isSame);
            
            ClearMaster();
            
            _masterCanvas.RenderForm(
                newForm, 
                () => _currentMaster = newForm.View);
            _masterCanvas.Title = newForm.Title;
            
            (newForm as IOnShownNeedingForm<HTMLElement>)?.OnShown();
        }
        
        public void Remove(IBareForm<HTMLElement> frm) {
            Logger.Debug(GetType(),"Removing form {0}", frm);

            if (frm.View == _currentMaster) {
                ClearMaster();
                return;
            }
            _parent.Remove(frm);
        }

        public void AddPopup(IBareForm<HTMLElement> newForm) => _parent.AddPopup(newForm);

        public IFormRenderer<HTMLElement> CreateRendererWithBase(IFormCanvas<HTMLElement> masterCanvas) {
            return new PopupDelegatingFormRenderer(this, masterCanvas);
        }
    }
}
