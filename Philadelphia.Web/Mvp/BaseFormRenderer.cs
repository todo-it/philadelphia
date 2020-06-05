using System;
using System.Collections.Generic;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class BaseFormRenderer : IFormRenderer<HTMLElement> {
        private IFormView<HTMLElement> _currentMaster;
        private IView<HTMLElement> _masterView;
        private readonly IFormCanvas<HTMLElement> _masterCanvas;

        private readonly IProvider<IFormCanvas<HTMLElement>> _popupCanvasProvider;
        private readonly IDictionary<IFormView<HTMLElement>, IFormCanvas<HTMLElement>> _popups 
            = new Dictionary<IFormView<HTMLElement>, IFormCanvas<HTMLElement>>();
        
        public BaseFormRenderer(IFormCanvas<HTMLElement> masterCanvas, IProvider<IFormCanvas<HTMLElement>> popupCanvasProvider) {
            _masterCanvas = masterCanvas;
            _popupCanvasProvider = popupCanvasProvider;
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

        public IFormRenderer<HTMLElement> CreateRendererWithBase(IFormCanvas<HTMLElement> masterCanvas) => 
            new PopupDelegatingFormRenderer(this, masterCanvas);

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
            
            (newForm as IOnShownNeedingForm<HTMLElement>)?.OnShown();
        }

        public void AddPopup(IBareForm<HTMLElement> newForm) {
            Logger.Debug(GetType(),"AddPopup() {0}", newForm);
            var newCanvas = _popupCanvasProvider.Provide();

            newCanvas.RenderForm(
                newForm, 
                () => {
                    if (_popups.ContainsKey(newForm.View)) {
                        Logger.Error(GetType(),"AddPopup() form was already added. master={0} popups={1} failing!", _currentMaster, _popups.PrettyToString());
                        throw new Exception(string.Format("form was already added {0}", newForm));
                    }

                    _popups.Add(newForm.View, newCanvas);
                });
            
            newCanvas.Title = newForm.Title;
            
            (newForm as IOnShownNeedingForm<HTMLElement>)?.OnShown();
        }
        
        public void Remove(IBareForm<HTMLElement> frm) {
            Logger.Debug(GetType(),"Remove() form {0}", frm);

            if (frm.View == _currentMaster) {
                ClearMaster();
                return;
            }

            if (_popups.ContainsKey(frm.View)) {
                var canvas = _popups[frm.View];

                canvas.Unrender();
                _popups.Remove(frm.View);
                
                return;
            }

            Logger.Error(GetType(),"Remove() form was not shown. master={0} popups={1} failing!", _currentMaster, _popups.PrettyToString());
            throw new Exception(string.Format("form was not shown {0}", frm));
        }
    }
}
