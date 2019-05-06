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
        event Action<Event,ConnectionReadyState> OnError;
    }

    public abstract class ServerSentEventsSubscriber<MsgT,CtxT> : IServerSentEventsSubscriber<MsgT> {
        private EventSource _evSrv;
        private readonly string _url;
        private bool _connectInvoked;
        public event Action<MsgT> OnMessage;
        public event Action OnConnOpen;
        public event Action<Event,ConnectionReadyState> OnError;

        protected ServerSentEventsSubscriber(
                bool autoConnect, Type serviceDecl, string subscrMethodName, CtxT ctx) {

            _url = "/" +serviceDecl.FullName + "/" + subscrMethodName+"?i="+JSON.Stringify(ctx);
            
            if (autoConnect) {
                _connectInvoked = true;
                Reconnect();
            }
        }

        public void Connect() {
            if (_connectInvoked) {
                throw new Exception("already attempting to connect");
            }
            _connectInvoked = true;
            Reconnect();
        }

        private void Reconnect() {
            _evSrv = new EventSource(_url);
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
