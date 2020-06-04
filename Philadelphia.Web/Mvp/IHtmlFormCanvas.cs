using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public interface IHtmlFormCanvas : IFormCanvas<HTMLElement> {
        string FormId { get; }
        bool IsShown { get; }
        HTMLElement ContainerElement { get; }
    }
}
