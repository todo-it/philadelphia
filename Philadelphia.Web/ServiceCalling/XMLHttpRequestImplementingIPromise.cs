using Bridge.Html5;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Philadelphia.Common;

namespace Philadelphia.Web {
    /// <summary>
    /// implementation note - errorhandler seems to not work. it throws strange errors withing setException being unable to cast something to array. Poor docs/no examples make it difficult to tell
    /// </summary>
    public class XMLHttpRequestImplementingIPromise : IPromise {
        private readonly string _method;
        private readonly string _url;
        private readonly FormData _frmData;
        
        public static string CsrfToken { get; set; }

        //for another example see https://forums.bridge.net/forum/bridge-net-pro/bugs/296-closed-372-1-7-exception-in-callback-within-promise-breaks-error-handling-in-task-frompromise
        
        public XMLHttpRequestImplementingIPromise(string method, string url, FormData frmData) {
            _method = method;
            _url = url;
            _frmData = frmData;
        }
        
        public void Then(Delegate fulfilledHandler, Delegate errorHandler = null, Delegate progressHandler = null) {
            var req = new XMLHttpRequest();
            
            req.OnReadyStateChange = () => {
                if (req.ReadyState != AjaxReadyState.Done) {
                    return;
                }
                
                if (req.Status >= 200 && req.Status < 400) {
                    Logger.Debug(GetType(), "upload success");
                    fulfilledHandler?.Call(null, ResultHolder<XMLHttpRequest>.CreateSuccess(req));
                    return;
                }
                
                Logger.Debug(GetType(), "upload error");
                fulfilledHandler?.Call(null, ResultHolder<XMLHttpRequest>.CreateFailure("", null, req));
            };
            req.Open(_method, _url, true);
            
            if (CsrfToken != null) {
                req.SetRequestHeader(Philadelphia.Common.Model.Magics.CsrfTokenFieldName, CsrfToken);
            }

            req.Send(_frmData);
        }
    }
}
