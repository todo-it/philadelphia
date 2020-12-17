using System;
using System.Collections.Generic;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public static class SyncViewExtensions {
        public static SyncModel<T,U> Bind<T,U>(
                this SyncView v,    
                ServerSentEventsSubscriber<T,U> l,
                Action<T> onMessage,
                Func<IEnumerable<string>,int,string> showLogsCaptionProvider,
                bool autoConnect,
                int maxLogLines) {
                
            var result = new SyncModel<T,U>(
                l, 
                onMessage, 
                maxLogLines,
                (x,cnt) => {
                    v.ShowLogsAction.Label = showLogsCaptionProvider(x,cnt);
                    v.LogItems.TextContent = string.Join("\n", x);
                },
                self => {
                    v.ToggleState.IsPressed = true;
                    v.State = SyncState.Connecting;

                    if (!l.ConnectionActive) {
                        self.Log(I18n.Translate("requested online (auto)"));
                        l.Connect();
                    } else {
                        self.Log(I18n.Translate("already requested online"));
                    }
                });
            
            v.State = SyncState.Offline;
            
            v.ToggleState.Triggered += () => {
                var isConnecting = v.ToggleState.IsPressed;

                if (isConnecting) {
                    v.State = SyncState.Connecting;

                    if (!l.ConnectionActive) {
                        result.Log(I18n.Translate("requested online (user)"));
                        l.Connect();
                    } else {
                        result.Log(I18n.Translate("already requested online"));
                    }

                    return;
                }

                result.Log(I18n.Translate("requested offline"));
                l.Disconnect();
                v.State = SyncState.Offline;
            };

            l.OnConnOpen += () => {
                v.State = SyncState.OnlineOk;
                result.Log(I18n.Translate("successfully connected"));
            };
            l.OnError += (ev, state) => {
                Logger.Debug(typeof(SyncViewExtensions), "onerror {0}", state);
                v.State = SyncState.OnlineButError;
                result.Log(string.Format(
                    I18n.Translate("connection error: {0}"),
                    state.GetUserFriendlyName() ));
            };

            if (autoConnect) {
                result.Connect();
            }

            return result;
        }
    }
}
