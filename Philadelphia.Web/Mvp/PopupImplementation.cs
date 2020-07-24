using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class PopupInfo {
        public IFormCanvas<HTMLElement> Canvas { get;}
        public IBareForm<HTMLElement> Form { get;}
        public IFormView<HTMLElement> FormView { get; }

        private PopupInfo(
                IFormCanvas<HTMLElement> canvas, IBareForm<HTMLElement> form, IFormView<HTMLElement> formView) {

            Canvas = canvas;
            Form = form;
            FormView = formView;
        }

        public static PopupInfo Create(
            IFormCanvas<HTMLElement> canvas, IBareForm<HTMLElement> form, IFormView<HTMLElement> formView) =>
                new PopupInfo(canvas, form, formView);
    }

    public class PopupImplementation {
        private readonly List<PopupInfo> _popups = new List<PopupInfo>();
        private readonly IProvider<IFormCanvas<HTMLElement>> _popupCanvasProvider;

        public PopupImplementation(IProvider<IFormCanvas<HTMLElement>> popupCanvasProvider) {
            _popupCanvasProvider = popupCanvasProvider;
        }

        public PopupInfo MaybeTopMostPopup => _popups.LastOrDefault();
        
        public void Remove(IBareForm<HTMLElement> frm) {
            var maybeFrm = _popups.FirstOrDefault(x => x.FormView == frm.View);
            
            if (maybeFrm != null) {
                maybeFrm.Canvas.Unrender();
                _popups.Remove(maybeFrm);
                
                return;
            }

            Logger.Error(GetType(),"Remove() form was not shown. Popups={0} failing!", _popups.PrettyToString());
            throw new Exception(string.Format("form was not shown {0}", frm));
        }
        
        public IFormCanvas<HTMLElement> AddPopup(IBareForm<HTMLElement> newForm) {
            Logger.Debug(GetType(),"AddPopup() {0}", newForm);
            var newCanvas = _popupCanvasProvider.Provide();

            newCanvas.RenderForm(
                newForm, 
                () => {
                    if (_popups.Any(x => x.FormView == newForm.View)) {
                        Logger.Error(GetType(),"AddPopup() form was already added. popups={0} failing!", 
                            _popups.PrettyToString());
                        throw new Exception(string.Format("form was already added {0}", newForm));
                    }

                    _popups.Add(PopupInfo.Create(newCanvas, newForm, newForm.View));
                });
            
            newCanvas.Title = newForm.Title;
            
            (newForm as IOnShownNeedingForm<HTMLElement>)?.OnShown();
            return newCanvas;
        }
    }
}
