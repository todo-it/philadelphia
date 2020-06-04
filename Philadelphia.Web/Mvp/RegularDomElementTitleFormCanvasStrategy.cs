using Bridge.Html5;

namespace Philadelphia.Web {
    public class RegularDomElementTitleFormCanvasStrategy : ITitleFormCanvasStrategy {
        private readonly HTMLDivElement _el;
        private readonly IHtmlFormCanvas _for;

        public RegularDomElementTitleFormCanvasStrategy(IHtmlFormCanvas createFor) {
            //title needs to be in container as we need margin in styling. 
            //Margins are not reflected in neither ClientHeight nor OffsetHeight and one needs to use slow/unreliable
            //http://stackoverflow.com/questions/10787782/full-height-of-a-html-element-div-including-border-padding-and-margin

            _for = createFor;
            _el = new HTMLDivElement();
            _el.SetAttribute(Magics.AttrDataFormId, createFor.FormId);
            _el.SetValuelessAttribute(Magics.AttrDataFormTitle);
        }

        public string Title { 
            set => _el.ReplaceChildren(
                !string.IsNullOrEmpty(value) 
                ? new [] { new HTMLDivElement{TextContent = value}} 
                : new HTMLDivElement[]{} );
        }

        public void OnCanvasHiding() => _for.ContainerElement.RemoveChild(_el);
        public void OnCanvasShowing() => _for.ContainerElement.AppendChild(_el);
    }
}
