using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class BodyBasedPropagatesToAppBatTitleFormCanvasStrategy : ITitleFormCanvasStrategy {
        private readonly IHtmlFormCanvas _for;
        private readonly HTMLDivElement _el;
        private string _title;

        private bool IsBodyBased => _for.ContainerElement == Document.Body;
        
        public BodyBasedPropagatesToAppBatTitleFormCanvasStrategy(IHtmlFormCanvas createFor) {
            _for = createFor;
            _el = new HTMLDivElement();
            _el.SetAttribute(Magics.AttrDataFormId, createFor.FormId);
            _el.SetValuelessAttribute(Magics.AttrDataFormTitle);  
        }
        
        public string Title {
            set {
                _title = value;
                _el.TextContent = _title;
                
                if (!_for.IsShown || !IsBodyBased) {
                    return;
                }
                
                Document.Title = _title;
            }
        }
        
        public void OnCanvasHiding() {
            _for.ContainerElement.MaybeRemoveChild(_el);
        }

        public void OnCanvasShowing() {
            Logger.Debug(GetType(), $"isBodyBased?={IsBodyBased}");
            
            if (IsBodyBased) {
                Document.Title = _title;
            } else {
                _for.ContainerElement.AppendChild(_el);    
            }
        }
    }
}
