using System;
using Bridge.Html5;

namespace Philadelphia.Web {
    public static class EventExtensions {
        public static bool IsUserGenerated(this Event self, bool dflt = true) {
            try {
                return self.IsTrusted;
            } catch(Exception) {
                return dflt;
            }
        }

        public static bool HasHtmlTarget(this Event self) {
            return self.Target.Is<HTMLElement>();
        }

        /// <summary> element that caused event to be raised (not neccessarly the one that eventlistener was subscribed to)</summary>
        public static HTMLElement HtmlTarget(this Event self) {
            return (HTMLElement)self.Target;
        }

        public static bool HasHtmlCurrentTarget(this Event self) {
            return self.CurrentTarget.Is<HTMLElement>();
        }

        /// <summary> element that has event subscribed</summary>
        public static HTMLElement HtmlCurrentTarget(this Event self) {
            return (HTMLElement)self.CurrentTarget;
        }
    }
}
