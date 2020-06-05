using System;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class FormRenderer : IFormRenderer<HTMLElement> {
        private IView<HTMLElement> _masterView;
        private readonly IFormCanvas<HTMLElement> _masterCanvas;
        private readonly PopupImplementation _popups;
        private readonly FormRenderer _parent;
        private IFormView<HTMLElement> _currentMaster;

        public FormRenderer(IFormCanvas<HTMLElement> masterCanvas, IProvider<IFormCanvas<HTMLElement>> popupCanvasProvider, FormRenderer parentRenderer = null) {
            _masterCanvas = masterCanvas;
            
            if ((popupCanvasProvider == null && parentRenderer == null) || 
                (popupCanvasProvider != null && parentRenderer != null)) {
                throw new Exception("either has to implement popups itself or delegate to parentRenderer");    
            }

            if (popupCanvasProvider != null) {
                _parent = null;
                _popups = new PopupImplementation(popupCanvasProvider);
            } else {
                _parent = parentRenderer;
                _popups = null;
            }
        }

        private IFormCanvas<HTMLElement> MaybeGetTopMostDialog() => 
            _parent != null ? _parent.MaybeGetTopMostDialog() : _popups.MaybeGetTopMostDialog();

        private void RemovePopupImpl(IBareForm<HTMLElement> frm) {
            if (_parent != null) {
                _parent.RemovePopupImpl(frm);
                return;
            }
            
            _popups.Remove(frm);
            TryPutFocusOnTopMostForm();
        }

        private void AddPopupImpl(IBareForm<HTMLElement> frm) {
            if (_parent != null) {
                _parent.AddPopupImpl(frm);
                return;
            } 
            
            var x = _popups.AddPopup(frm);
            TryPutFocusOnForm(x);
        }

        private void TryPutFocusOnForm(IFormCanvas<HTMLElement> frmCnv) {
            Logger.Debug(GetType(), "TryPutFocusOnForm() frmCnv={0}", frmCnv);
            
            Logger.Error(GetType(), "maybe needs to steal focus from glass covered element {0}", Document.ActiveElement);
            Document.ActiveElement?.Blur();
            
            frmCnv.Focus();
        }

        private void TryPutFocusOnTopMostForm() {
            var frmCnvOrNull =  MaybeGetTopMostDialog();

            if (frmCnvOrNull != null) {
                Logger.Debug(GetType(),"TryPutFocusOnTopMostForm() will use dialog");
                TryPutFocusOnForm(frmCnvOrNull);
                return;
                
            } 
            
            if (_currentMaster != null) {
                Logger.Debug(GetType(),"TryPutFocusOnTopMostForm() will use master");
                TryPutFocusOnForm(_masterCanvas);
                return;
            }
            
            Logger.Debug(GetType(),"TryPutFocusOnTopMostForm() no forms left");
        }

        public void ClearMaster() {
            Logger.Debug(GetType(),"ClearMaster() _currentMaster={0} _masterView={1}", _currentMaster, _masterView);
            
            if (_masterView != null) {
                _masterCanvas.Hide();
                _masterView = null;
            }

            if (_currentMaster == null) {
                return;
            }
            
            _masterCanvas.Unrender();
            _currentMaster = null;
        }

        public void ReplaceMasterWithAdapter(IView<HTMLElement> newItem) {
            Logger.Debug(GetType(),"ReplaceMasterWithAdapter() with {0}", newItem);
            ClearMaster();
			
            _masterCanvas.RenderAdapter(newItem);
            _masterView = newItem;
        }

        public void ReplaceMaster(IBareForm<HTMLElement> newForm) {
            var isSame = newForm.View == _currentMaster;
            Logger.Debug(GetType(),"ReplaceMaster() with {0}. Same as now?{0}", newForm, isSame);
            
            ClearMaster();
			
            _masterCanvas.RenderForm(
                newForm, 
                () => _currentMaster = newForm.View);
            _masterCanvas.Title = newForm.Title;

            //don't steal focus from popup (if any) 
            if (MaybeGetTopMostDialog() == null) {
                TryPutFocusOnForm(_masterCanvas);    
            }

            (newForm as IOnShownNeedingForm<HTMLElement>)?.OnShown();
        }

        public void Remove(IBareForm<HTMLElement> frm) {
            Logger.Debug(GetType(), "Remove() form={0} _currentMaster={1} isMaster?={2}", 
                frm, _currentMaster, frm.View == _currentMaster);

            if (frm.View == _currentMaster) {
                ClearMaster();
                //if there's popup it keeps focus
                return;
            }

            RemovePopupImpl(frm);
        }

        public void AddPopup(IBareForm<HTMLElement> newForm) => AddPopupImpl(newForm);
        
        public IFormRenderer<HTMLElement> CreateRendererWithBase(IFormCanvas<HTMLElement> masterCanvas) =>
            new FormRenderer(masterCanvas, null, this);
    }
}
