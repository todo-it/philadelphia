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
            if (_masterView != null) {
                Logger.Debug(typeof(FormCanvasExtensions), "Cleaning IView from masterCanvas");
                _masterCanvas.Hide();
                _masterView = null;
            }

            if (_currentMaster == null) {
                return;
            }
            
            Logger.Debug(GetType(),"Removing master");
            _masterCanvas.Unrender();
            _currentMaster = null;
        }

        public IFormRenderer<HTMLElement> CreateRendererWithBase(IFormCanvas<HTMLElement> masterCanvas) => 
            new PopupDelegatingFormRenderer(this, masterCanvas);

        public HTMLElement BaseCanvasElement => _masterCanvas.Body.ParentElement;

        public void ReplaceMasterWithAdapter(IView<HTMLElement> newItem) {
            Logger.Debug(GetType(),"Replacing master with {0}", newItem);
            ClearMaster();
			
            _masterCanvas.Render(newItem);
            _masterView = newItem;
        }

        public void ReplaceMaster(IBareForm<HTMLElement> newForm) {
            var isSame = newForm.View == _currentMaster;
            Logger.Debug(GetType(),"Replacing master with {0}. Same as now?{0}", newForm, isSame);
            
            ClearMaster();
			
            _masterCanvas.Render(
                newForm, 
                () => _currentMaster = newForm.View);
            _masterCanvas.Title = newForm.Title;
            
            (newForm as IOnShownNeedingForm<HTMLElement>)?.OnShown();
        }

        public void AddPopup(IBareForm<HTMLElement> newForm) {
            Logger.Debug(GetType(),"Adding popup {0}", newForm);
            var newCanvas = _popupCanvasProvider.Provide();

            newCanvas.Render(
                newForm, 
                () => _popups.Add(newForm.View, newCanvas));
            
            newCanvas.Title = newForm.Title;
            
            (newForm as IOnShownNeedingForm<HTMLElement>)?.OnShown();
        }
        
        public void Remove(IBareForm<HTMLElement> frm) {
            Logger.Debug(GetType(),"Removing form {0}", frm);

            if (frm.View == _currentMaster) {
                ClearMaster();
                return;
            }

            if (_popups.ContainsKey(frm.View)) { 
                Logger.Debug(GetType(),"Removing popup {0}", frm);
                var canvas = _popups[frm.View];

                canvas.Unrender();
                _popups.Remove(frm.View);
                return;
            }

            Logger.Debug(GetType(),"form was not shown. master={0} popups={1} failing!", _currentMaster, _popups.PrettyToString());
            throw new Exception(string.Format("form was not shown {0}", frm));
        }
    }
}
