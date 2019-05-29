using System;
using Bridge.Html5;
using Philadelphia.Common;
using Philadelphia.Demo.SharedModel;

namespace Philadelphia.Demo.Client {
    public class SseSenderForm : IForm<HTMLElement,SseSenderForm,Unit> {
        private readonly Func<string> _sseSessionIdProvider;
        public event Action<SseSenderForm, Unit> Ended;
        private readonly SseSenderFormView _view = new SseSenderFormView();
        public string Title => "Publisher";
        public IFormView<HTMLElement> View => _view;
        
        public SseSenderForm(ISomeService someService, Func<string> sseSessionIdProvider) {
            _sseSessionIdProvider = sseSessionIdProvider;
            RemoteActionBuilder.Build(_view.PublishCanada, 
                () => someService.PublishNotification(_sseSessionIdProvider(), Country.Canada), 
                x => LogAccept(Country.Canada, x));
            RemoteActionBuilder.Build(_view.PublishUSA, 
                () => someService.PublishNotification(_sseSessionIdProvider(), Country.USA), 
                x => LogAccept(Country.USA, x));
            RemoteActionBuilder.Build(_view.PublishGermany, 
                () => someService.PublishNotification(_sseSessionIdProvider(), Country.Germany), 
                x => LogAccept(Country.Germany, x));
            RemoteActionBuilder.Build(_view.PublishFrance, 
                () => someService.PublishNotification(_sseSessionIdProvider(), Country.France), 
                x => LogAccept(Country.France, x));
            RemoteActionBuilder.Build(_view.PublishSouthAfrica, 
                () => someService.PublishNotification(_sseSessionIdProvider(), Country.SouthAfrica), 
                x => LogAccept(Country.SouthAfrica, x));
            RemoteActionBuilder.Build(_view.PublishTunisia, 
                () => someService.PublishNotification(_sseSessionIdProvider(), Country.Tunisia), 
                x => LogAccept(Country.Tunisia, x));
        }

        public ExternalEventsHandlers ExternalEventsHandlers => ExternalEventsHandlers.Create(() => Ended?.Invoke(this, Unit.Instance));

        private void LogAccept(Country c, DateTime at) {
            Log("Server accepted notification about {0} at {1}", 
                c.ToString(), I18n.Localize(at, DateTimeFormat.YMDhms));
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
            var timestamp = string.Format("{0:0000}-{1:00}-{2:00} {3:00}:{4:00}:{5:00}", 
                t.Year, t.Month, t.Day, t.Hour, t.Minute, t.Second);
            
            _view.HistoryLog.ContentElement.InsertBefore(
                new HTMLSpanElement {TextContent = timestamp + " " + msg}, 
                _view.HistoryLog.ContentElement.FirstChild);
        }
    }
}
