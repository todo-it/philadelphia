using System;
using Bridge.Html5;

namespace Philadelphia.Web {
    public interface IServerSentEventsSubscriber<MsgT> : IDisposable {
        event Action<MsgT> OnMessage;
        event Action OnConnOpen;
        event Action<string> OnStreamIdAssigned;
        event Action<Event,ConnectionReadyState> OnError;
        string SseStreamId {get;}
    }
}
