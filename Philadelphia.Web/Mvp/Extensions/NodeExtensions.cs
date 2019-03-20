using System.Collections.Generic;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public static class NodeExtensions {
        public static HTMLElement AsElementOrNull(this Node self) {
            return self.NodeType == NodeType.Element ? (HTMLElement)self : null;
        }

        public static void AppendManyChildren(this Node self, IEnumerable<Node> elems) {
            elems.ForEach(x => self.AppendChild(x));
        }
    }
}
