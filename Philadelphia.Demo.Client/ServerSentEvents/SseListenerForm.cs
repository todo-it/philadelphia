using System;
using System.Collections.Generic;
using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Demo.SharedModel;
using Philadelphia.Web;

namespace Philadelphia.Demo.Client {
    public class SseListenerForm : IForm<HTMLElement,SseListenerForm,Unit> {
        private readonly Func<ContinentalSubscriptionRequest, IServerSentEventsSubscriber<ContinentalNotification>> _listenerBld;
        private IServerSentEventsSubscriber<ContinentalNotification> _listener;
        public event Action<SseListenerForm, Unit> Ended;
        private readonly SseListenerFormView _view = new SseListenerFormView();
        private Continent? _activeCont;
        private IDictionary<Continent,LocalActionModel> _contToAction 
            = new Dictionary<Continent, LocalActionModel>();

        private readonly LocalActionModel _unsubscribe;

        public string Title => "Subscriber/Listener";
        public IFormView<HTMLElement> View => _view;
        
        public SseListenerForm(
            Func<ContinentalSubscriptionRequest,IServerSentEventsSubscriber<ContinentalNotification>> listenerBld) {

            _listenerBld = listenerBld;
            _unsubscribe = LocalActionBuilder.Build(_view.Unsubscribe, () => SetupListener(null));
            
            _contToAction.Add(Continent.Africa, 
                LocalActionBuilder.Build(_view.SubscribeAfrica, 
                    () => SetupListener(Continent.Africa)));

            _contToAction.Add(Continent.Antarctica, 
                LocalActionBuilder.Build(_view.SubscribeAntarctica, 
                    () => SetupListener(Continent.Antarctica)));
            
            _contToAction.Add(Continent.Europe, 
                LocalActionBuilder.Build(_view.SubscribeEurope, 
                    () => SetupListener(Continent.Europe)));
                        
            _contToAction.Add(Continent.NorthAmerica, 
                LocalActionBuilder.Build(_view.SubscribeNorthAmerica, 
                    () => SetupListener(Continent.NorthAmerica)));
            
            SetupEnablement();
        }

        public ExternalEventsHandlers ExternalEventsHandlers => ExternalEventsHandlers.Ignore;

        private void SetupListener(Continent? cont) {
            if (_listener != null) {
                _listener.Dispose();
                _listener = null;
                Log("Unsubscribed from : {0}", _activeCont.Value.ToString());
                _activeCont = null;
            }
            
            SetupEnablement();

            if (!cont.HasValue) {
                return;
            }

            _listener = _listenerBld(new ContinentalSubscriptionRequest {Continent = cont.Value});
            _activeCont = cont;

            SetupEnablement();

            _listener.OnMessage += msg => {
                msg.PostDeserializationFix();
                
                Log("Got notification from {0} at {1} about country={2}",
                    msg.Sender, I18n.Localize(msg.SentAt, DateTimeFormat.YMDhms), msg.Country.ToString());
            };
            _listener.OnError += (_,rs) => {
                if (rs == ConnectionReadyState.CLOSED) {
                    //f.e. httpstatus rejected

                    Log("Permanent connection error - unsubscribing");
                    SetupListener(null);
                    return;
                } 

                Log("Temporary connection error - reconnecting");
            };
            _listener.OnConnOpen += () => Log("Successfully connected and subscribed");

            Log("Requested subscription to {0}", cont.Value.ToString());
        }

        private void SetupEnablement() {
            if (_activeCont.HasValue) {
                _unsubscribe.ChangeEnabled(true, new string[0], false);
            } else {
                _unsubscribe.ChangeEnabled(false, new []{"Not subscribed yet"}, false);
            }
            
            foreach (var contToAct in _contToAction) {
                if (_activeCont.HasValue) {
                    contToAct.Value.ChangeEnabled(false, new [] {"Already subscribed"}, false);
                } else {
                    contToAct.Value.ChangeEnabled(true, new string[0], false);
                }
            }
        }

        private void Log(string msg, params object[] args) {
            if (args.Length > 0) {
                try {
                    msg = string.Format(msg, args);
                } catch(Exception ex) {
                    msg = msg +" unformatted due to exception: "+ex;
                }
            }
            
            var t = DateTime.Now;
            _view.HistoryLog.ContentElement.InsertBefore(new HTMLSpanElement {
                    TextContent = I18n.Localize(t, DateTimeFormat.YMDhms) +" " + msg}, 
                _view.HistoryLog.ContentElement.FirstChild);
        }
    }
}
