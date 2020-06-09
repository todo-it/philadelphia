using System;
using Bridge.Html5;

namespace Philadelphia.Web {
    public enum EnvironmentType {
        Desktop,
        IndustrialAndroidWebApp
    }
    
    public static class EnvironmentTypeUtil {
        public static EnvironmentType GetInstanceFromWindow(WindowInstance win) =>
            (win.Navigator.UserAgent.Contains("IndustrialAndroidWebApp") || 
             BridgeObjectUtil.HasFieldOrMethod(win, "IAWApp")) 
                ? EnvironmentType.IndustrialAndroidWebApp
                : EnvironmentType.Desktop;

        public static string AsDataEnvironmentAttributeValue(this EnvironmentType self) {
            switch (self) {
                case EnvironmentType.Desktop: return "desktop";
                case EnvironmentType.IndustrialAndroidWebApp: return "iawapp";
                default: throw new Exception("unsupported environment");
            }
        }
    }
}
