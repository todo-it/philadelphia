using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class PopupImplementation {
        private readonly List<Tuple<IFormView<HTMLElement>, IFormCanvas<HTMLElement>>> _popups 
            = new List<Tuple<IFormView<HTMLElement>, IFormCanvas<HTMLElement>>>();
        private readonly IProvider<IFormCanvas<HTMLElement>> _popupCanvasProvider;

        public PopupImplementation(IProvider<IFormCanvas<HTMLElement>> popupCanvasProvider) {
            _popupCanvasProvider = popupCanvasProvider;
        }

        public IFormCanvas<HTMLElement> MaybeGetTopMostDialog() => _popups.LastOrDefault()?.Item2;
        
        public void Remove(IBareForm<HTMLElement> frm) {
            var maybeFrm = _popups.FirstOrDefault(x => x.Item1 == frm.View);
            
            if (maybeFrm != null) {
                maybeFrm.Item2.Unrender();
                _popups.Remove(maybeFrm);
                
                return;
            }

            Logger.Error(GetType(),"Remove() form was not shown.popups={0} failing!", _popups.PrettyToString());
            throw new Exception(string.Format("form was not shown {0}", frm));
        }
        
        public IFormCanvas<HTMLElement> AddPopup(IBareForm<HTMLElement> newForm) {
            Logger.Debug(GetType(),"AddPopup() {0}", newForm);
            var newCanvas = _popupCanvasProvider.Provide();

            newCanvas.RenderForm(
                newForm, 
                () => {
                    if (_popups.Any(x => x.Item1 == newForm.View)) {
                        Logger.Error(GetType(),"AddPopup() form was already added. popups={0} failing!", 
                            _popups.PrettyToString());
                        throw new Exception(string.Format("form was already added {0}", newForm));
                    }

                    _popups.Add(Tuple.Create(newForm.View, newCanvas));
                });
            
            newCanvas.Title = newForm.Title;
            
            (newForm as IOnShownNeedingForm<HTMLElement>)?.OnShown();
            return newCanvas;
        }
    }
}
