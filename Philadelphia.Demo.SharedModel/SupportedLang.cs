using System;

namespace Philadelphia.Demo.SharedModel {
    public enum SupportedLang {
        DE,
        EN,
        FR,
        PL
    }

    public static class SupportedLangExtensions {
        public static string GetLangName(this SupportedLang self) {
            switch (self) {
                case SupportedLang.EN: return "English";
                case SupportedLang.DE: return "Deutsch";
                case SupportedLang.FR: return "Français";
                case SupportedLang.PL: return "Polski";
                default: throw new Exception("unsupported language");
            }
        }
    }
}
