﻿using System;
using System.Collections.Generic;

namespace Philadelphia.Common {
    public class ClientConnectionInfo {
        private Func<string, string> _getCookieOrNull;
        private Action<Cookie> _setCookie;

        /// <summary>user visible connection identifier. By default it is Guid.ToString().
        /// You can override it by registering IClientConnectionInfoConnectionIdProvider in DI container</summary>
        public string ConnectionId { get; private set; }
        public string ClientIpAddress { get; private set; }
        
        public string ClientTimeZoneCodeOrNull { get; private set; }
        public int? ClientTimeZoneOffset { get; private set; }
        
        public string CsrfTokenOrNull { get; private set; }

        // REVIEW: Which value is provided?
        /// <summary>
        /// value is provided later by DI
        /// </summary>
        public void Initialize(
                Func<string,string> getCookieOrNull, 
                Action<Cookie> setCookie,
                string clientIpAddress, 
                string clientTimeZoneCodeOrNull = null,
                int? clientTimeZoneOffset = null) {
            
            _getCookieOrNull = getCookieOrNull;
            _setCookie = setCookie;
            ClientIpAddress = clientIpAddress;
            ClientTimeZoneCodeOrNull = clientTimeZoneCodeOrNull;
            ClientTimeZoneOffset = clientTimeZoneOffset;
        }

        public void InitializeConnectionId(string connId) {
            ConnectionId = connId;
        }

        public void InitializeCsrfToken(string token) {
            CsrfTokenOrNull = token;
        }
        
        public string GetCookieOrNull(string inp) {
            return _getCookieOrNull(inp);
        }

        public void SetCookie(Cookie inp) {
            _setCookie(inp);
        }
    }
}
