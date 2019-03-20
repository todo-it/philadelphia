using Bridge;

namespace Philadelphia.Web {
    /// <summary>the one included in Bridge.Html5 v15.7 doesn't have Keys() method</summary>
    [External]
    [Name("URLSearchParams")]
    public class URLSearchParams {
        public URLSearchParams() {}
        
        public virtual extern string get(string keyName);
        public virtual extern IIterable<string> keys();
    }
}
