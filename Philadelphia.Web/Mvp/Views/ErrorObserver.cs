using System;
using System.Collections.Generic;
using System.Linq;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class ErrorObserver {
        private readonly Action<int> _forwardErrorCount;
        private readonly ISet<object> _errors = new HashSet<object>();

        public ErrorObserver(Action<int> forwardErrorCount) {
            _forwardErrorCount = forwardErrorCount;
        }

        public void Update(object sender, ISet<string> errors) {
            if (!errors.Any()) {
                _errors.Remove(sender);
            } else {
                _errors.Add(sender);    
            }
                
            Logger.Debug(GetType(), "updating error count {0}", _errors.Count);
            _forwardErrorCount(_errors.Count);
        }
    }
}
