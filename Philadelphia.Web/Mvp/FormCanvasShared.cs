using System.Collections.Generic;
using System.Linq;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public static class FormCanvasShared {
        public static void AddActions(HTMLElement dst, IEnumerable<HTMLElement> src) {
            var leftAndRight = src.ToList().Partition(x => !x.HasAttribute(Magics.AttrAlignToRight));
    
            dst.ReplaceChildren(
                leftAndRight.Item1
                    .ConcatElementIfTrue(
                        leftAndRight.Item2.Any(),
                        DocumentUtil.CreateElementHavingClassName("span", Magics.CssClassFlexSpacer))
                    .Concat(leftAndRight.Item2));
        }
    }
}
