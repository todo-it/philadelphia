using System;
using Bridge;
using Bridge.Html5;

namespace Philadelphia.Web {
    [External]
    [Name("EventSource")]
    public class EventSource {
        public EventSource(string url) {}

        public int readyState;

        public Action onopen;
        public Action<MessageEvent> onmessage;
        public Action<Event> onerror;

        /// <summary>closes connection</summary>
        public virtual extern void close();
    }
}
