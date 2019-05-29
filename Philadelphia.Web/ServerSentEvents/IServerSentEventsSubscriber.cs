using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bridge.Html5;
using Philadelphia.Common;
using Newtonsoft.Json;

namespace Philadelphia.Web {
    public interface IServerSentEventsSubscriber<MsgT> : IDisposable {
        event Action<MsgT> OnMessage;
        event Action OnConnOpen;
        event Action<string> OnStreamIdAssigned;
        event Action<Event,ConnectionReadyState> OnError;
        string SseStreamId {get;}
    }

    public abstract class ServerSentEventsSubscriber<MsgT,CtxT> : IServerSentEventsSubscriber<MsgT> {
        private EventSource _evSrv = null;
        private readonly string _url;
        public event Action<MsgT> OnMessage;
        public event Action OnConnOpen;
        public event Action<string> OnStreamIdAssigned;
        public event Action<Event,ConnectionReadyState> OnError;
        public string SseStreamId {get; private set;}

        protected ServerSentEventsSubscriber(
                bool autoConnect, Type serviceDecl, string subscrMethodName, CtxT ctx) {

            _url = "/" +serviceDecl.FullName + "/" + subscrMethodName+"?i="+JSON.Stringify(ctx);
            
            if (autoConnect) {
                Reconnect();
            }
        }

        public void Disconnect() {
            if (_evSrv == null) {
                throw new Exception("it wasn't connected or attempting to be connected");
            }
            
            _evSrv.close();
            _evSrv = null;
        }
        
        public void Connect() {
            if (_evSrv != null) {
                throw new Exception("already attempting to connect");
            }
            Reconnect();
        }

        private void Reconnect() {
            _evSrv = new EventSource(_url);

            _evSrv.addEventListener(Common.Model.Magics.SseStreamIdEventName, ev => {
                SseStreamId = (string)ev.Data;
                Logger.Debug(GetType(), "SseStreamId is now {0}", SseStreamId);
                OnStreamIdAssigned?.Invoke(SseStreamId);
            });
            
            _evSrv.onmessage = ev => {
                var data = (string)ev.Data;
                Logger.Debug(GetType(), "got message: {0}", data);
                var recv = JsonConvert.DeserializeObject<MsgT>(data);
                Logger.Debug(GetType(), "parsed as: {0}", recv);
                OnMessage?.Invoke(recv);
            };
            
            _evSrv.onopen = () => {
                Logger.Debug(GetType(), "conn opened");
                OnConnOpen?.Invoke();
            };

            _evSrv.onerror = ev => {
                Logger.Debug(GetType(), "conn error {0} state {1}", ev, _evSrv.readyState.ToString());
                OnError?.Invoke(ev, _evSrv.GetRichReadyState());
            };
        }

        public void Dispose() {
            if (_evSrv == null) {
                return;
            }

            _evSrv.close();
            _evSrv = null;
        }
    }
}
