using System.Collections.Generic;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public static class ViewExtensions {
        /// <summary>fluent style to make it usable during fields initialization (avoids constructors for formviews)</summary>
        public static T WithStyle<T>(this T self, Dictionary<string,string> elems) where T : IView<HTMLElement> {
            self.Widget.SetStyle(elems);
            return self;
        }

        /// <summary>fluent style to make it usable during fields initialization (avoids constructors for formviews)</summary>
        public static T WithCssClass<T>(this T self, string cssClassname) where T : IView<HTMLElement> {
            self.Widget.ClassList.Add(cssClassname);
            return self;
        }
        
        public static bool IsAttached(this IView<HTMLElement> self) {
            return self.Widget.IsAttached();
        }
    }
}
