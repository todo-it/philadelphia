using System;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public static class ConnectionReadyStateExtensions {
        public static string GetUserFriendlyName(this ConnectionReadyState self) {
            switch (self) {
                case ConnectionReadyState.CONNECTING: return I18n.Translate("connecting");
                case ConnectionReadyState.OPEN: return I18n.Translate("connected");
                case ConnectionReadyState.CLOSED: return I18n.Translate("disconnected");
                default: throw new Exception("unsupported ConnectionReadyState");
            }
        }
    }
}
