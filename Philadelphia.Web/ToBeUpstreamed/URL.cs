using Bridge;

namespace Philadelphia.Web {
    [External]
    [Name("URL")]
    public class URL {
        public URL(string urlToParse) {}

        public string hash;
        public string host;
        public string hostname;
        public string href;
        public readonly string origin;
        public string password;
        public string pathname;
        public string port;
        public string protocol;
        public string search;

        public URLSearchParams searchParams;
        public string username;
    }
}
