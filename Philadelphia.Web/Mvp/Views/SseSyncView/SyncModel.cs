using System;
using System.Collections.Generic;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class SyncModel<T,U> {
        private readonly ServerSentEventsSubscriber<T, U> _listener;
        private readonly int _maxLogLines;
        private readonly Action<List<string>,int> _onLogsChanged;
        private readonly Action<SyncModel<T, U>> _onConnect;
        private readonly List<string> _logs = new List<string>();
        private int _logMessageCounter = 0;
        public string SseStreamId => _listener.SseStreamId;

        public SyncModel(
            ServerSentEventsSubscriber<T,U> listener, 
            Action<T> onMessage, 
            int maxLogLines,
            Action<List<string>,int> onLogsChanged,
            Action<SyncModel<T,U>> onConnect) {
                
            _listener = listener;
            _maxLogLines = maxLogLines;
            _onLogsChanged = onLogsChanged;
            _onConnect = onConnect;

            _listener.OnMessage += x => {
                Logger.Debug(GetType(), "received mutation {0}", x);
                onMessage(x);
            };
        }
        
        public void Log(string msg) {
            _logMessageCounter++;
            _logs.Add(_logMessageCounter + " " + DateTime.Now.ToStringYyyyMmDdHhMmSs() + " " + msg);

            while (_logs.Count > _maxLogLines) {
                _logs.RemoveAt(0);
            }

            _onLogsChanged(_logs, _logMessageCounter);
        }

        public void Connect() {
            _onConnect(this);
        }
        
        public void ConnectIfNeeded() {
            _onConnect(this);
        }
    }
}
