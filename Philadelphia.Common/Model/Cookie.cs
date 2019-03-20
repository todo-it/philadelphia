using System;

namespace Philadelphia.Common {
    /// <summary>abstraction so that we don't expose aspnet core</summary>
    public class Cookie {
        public string Name {get; set; }
        public string Value {get; set; }

        public bool HttpOnly {get; set; } = true;
        public bool Secure {get; set; }
        public DateTimeOffset? Expires {get; set; }
        public TimeSpan? MaxAge {get; set; }
        public string Path {get; set; }
    }
}
