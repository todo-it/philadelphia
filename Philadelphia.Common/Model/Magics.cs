using System;
using System.Collections.Generic;
using System.Text;

namespace Philadelphia.Common.Model {
    public static class Magics {
        public const string CsrfTokenFieldName = "X-CSRF-Token";
        public const string TimeZoneCodeFieldName = "X-TimeZone-Code";
        public const string TimeZoneOffsetFieldName = "X-TimeZone-Offset";
    }
}
