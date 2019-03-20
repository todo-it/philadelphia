using System;
using System.Collections.Generic;

namespace Philadelphia.Common {
    public class ClientConnectionInfo {
        private Func<string, string> _getCookieOrNull;
        private Action<Cookie> _setCookie;

        public string ClientIpAddress { get; private set; }
        public string CsrfToken { get; private set; }

        // REVIEW: Which value is provided?
        /// <summary>
        /// value is provided later by DI
        /// </summary>
        public void Initialize(
                string clientIpAddress, Func<string,string> getCookieOrNull, Action<Cookie> setCookie) {

            ClientIpAddress = clientIpAddress;
            _getCookieOrNull = getCookieOrNull;
            _setCookie = setCookie;
        }

        public void InitializeCsrfToken(string token) {
            CsrfToken = token;
        }
        
        public string GetCookieOrNull(string inp) {
            return _getCookieOrNull(inp);
        }

        public void SetCookie(Cookie inp) {
            _setCookie(inp);
        }
    }
}
