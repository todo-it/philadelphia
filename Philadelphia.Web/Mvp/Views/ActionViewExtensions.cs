using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public static class ActionViewExtensions {
        public static void MarkAsFormsDefaultButton(this IActionView<HTMLElement> action) {
            action.Widget.SetAttribute(Magics.AttrDataFormDefaultAction, "");
        }
    }
}
