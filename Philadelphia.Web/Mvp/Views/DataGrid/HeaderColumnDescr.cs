using Bridge.Html5;

namespace Philadelphia.Web {
    public class HeaderColumnDescr {
        public Element LabelElem {get; }
        public Element FilterElem {get; }
        public bool Orderable { get; }
        public bool IsSelectionHandler { get; }

        public HeaderColumnDescr(Element labelElement, Element filterElem, bool orderable, bool isSelectionHandler) {
            LabelElem = labelElement;
            FilterElem = filterElem;
            Orderable = orderable;
            IsSelectionHandler = isSelectionHandler;
        }
    }
}
