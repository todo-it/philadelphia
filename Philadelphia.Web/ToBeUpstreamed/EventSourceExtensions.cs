namespace Philadelphia.Web {
    public static class EventSourceExtensions {
        public static ConnectionReadyState GetRichReadyState(this EventSource self) {
            //https://www.w3.org/TR/eventsource/#dom-eventsource-readystate
            return (ConnectionReadyState)self.readyState;
        }
    }
}
