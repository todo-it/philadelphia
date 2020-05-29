using System;

namespace Philadelphia.Web {
    public enum IconFontType {
        FontAwesomeSolid,
        FontAwesomeRegular,
        FontAwesomeBrands
    }
    
    public static class IconFontTypeExtensions {
        public static string ToCssClassName(this IconFontType self) {
            switch (self) {
                case IconFontType.FontAwesomeSolid: return "faSolid";
                case IconFontType.FontAwesomeRegular: return "faRegular";
                case IconFontType.FontAwesomeBrands: return "faBrands";
                default: throw new Exception("unsupported FontAwesomeType");
            }
        }
    }
}
