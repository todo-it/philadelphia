using System;
using System.Threading.Tasks;
using Bridge.Html5;
using Philadelphia.Common;
using Bridge;
using Newtonsoft.Json;

namespace Philadelphia.Web {
    public class HttpRequester : IHttpRequester {
        
        public static readonly IHttpRequester Instance = new HttpRequester();

        [Template("typeof({data}) === \"object\"")]
        public static extern bool IsSentAsProperJson(object data);
        
        public async Task<OutpT> RunHttpRequest<InpT,OutpT>(
                string interfaceName, string methodName, Func<string,OutpT> deserialize, InpT inp) {

            var inputAsJson = JSON.Stringify(inp);
            var requestId = Guid.NewGuid().ToString();

            var url = string.Format("/{0}/{1}", interfaceName, methodName);
			Logger.Debug(typeof(HttpRequester), "Request id={0} to={1} starting", requestId, url);
			var result = await Task.FromPromise<ResultHolder<XMLHttpRequest>>(
                new XMLHttpRequestImplementingIPromise("POST", url, inputAsJson),
                (Func<ResultHolder<XMLHttpRequest>, ResultHolder<XMLHttpRequest>>)(x => x));

            if (result.Success) {
                Logger.Debug(typeof(HttpRequester), "Request id={0} Success now will deserialize", requestId);
                var bsd = deserialize(BridgeObjectUtil.NoOpCast<string>(result.Result.ResponseText));
                Logger.Debug(typeof(HttpRequester), "Success ok deserialized");
								
                return bsd;
            } 

            var answer = result.Result.ResponseText;
            Logger.Error(
                typeof(HttpRequester), 
                $"Failed request id={requestId} while calling server. Got status={result.Result.Status} Response={answer}");
            var errMsg = result.Result.Status == 400 ? answer.TillFirstNewLineOrEverything() : answer;
            throw new Exception(errMsg);
        }

        public Task<FileModel> RunHttpRequestReturningAttachmentImplStr(string interfaceName, string methodName, string inputParam) {
            var url = string.Format("/{0}/{1}", interfaceName, methodName);
            
            Logger.Debug(typeof(HttpRequester), "POST via iframe url={0} params={1}", url, inputParam);

            //create form post within iframe. This will download file and should put eventual error page into new window (thus not breaking singlepageapp too hard)
            
            var iframe = new HTMLIFrameElement();
            iframe.Style.Visibility = Visibility.Hidden;

            Document.Body.AppendChild(iframe);

            var form = new HTMLFormElement();
            form.Target = "_blank";
            form.Method = "POST";
            form.Action = url;
            
            form.AppendChild(new HTMLInputElement {
                Type = InputType.Hidden,
                Name = Magics.PostReturningFileParameterName,
                Value = inputParam });

            if (Toolkit.CsrfToken != null) {
                form.AppendChild(new HTMLInputElement {
                    Type = InputType.Hidden,
                    Name = Philadelphia.Common.Model.Magics.CsrfTokenFieldName,
                    Value = Toolkit.CsrfToken });
            }
            
            iframe.AppendChild(form);
            form.Submit();
            Document.Body.RemoveChild(iframe);

            return Task.FromResult(
                FileModel.CreateDownloadRequest(
                    url, 
                    new Tuple<string, string>(
                        Magics.PostReturningFileParameterName, 
                        inputParam)));
        }
        
        public string BuildUrl(string interfaceName, string methodName, string inputParam) {
            var result = 
                string.Format(
                    "/{0}/{1}?{2}=", interfaceName, methodName, Magics.PostReturningFileParameterName) + 
                Window.EncodeURIComponent(inputParam);

            Logger.Debug(typeof(HttpRequester), "built GET url={0}", result);

            return result;
        }

        public T DeserializeObject<T>(string input) {
            return JsonConvert.DeserializeObject<T>(input);
        }

        public string SerializeObject<T>(T input) {
            return JSON.Stringify(input);
        }
    }
}
