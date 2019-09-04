using System;
using System.Threading.Tasks;
using Bridge.Html5;
using Philadelphia.Common;
using Bridge;
using Newtonsoft.Json;

namespace Philadelphia.Web {
    public class HttpRequester {
        public static string CsrfToken => Toolkit.CsrfToken;

        private static T NullAwareJsonParse<T>(string input) {
            return "null".Equals(input) ? default(T) : JsonConvert.DeserializeObject<T>(input);
        }
        
        [Template("typeof({data}) === \"object\"")]
        public static extern bool IsSentAsProperJson(object data);
        
        public static Task<OutpT> RunHttpRequest<OutpT>(string interfaceName, string methodName, Func<string,OutpT> deserialize) {
            return RunHttpRequest(interfaceName, methodName, deserialize, "");
        }

        public static Task<OutpT> RunHttpRequest<InpT1,InpT2,InpT3,InpT4,InpT5,OutpT>(string interfaceName, string methodName, Func<string,OutpT> deserialize, InpT1 inp1, InpT2 inp2, InpT3 inp3, InpT4 inp4, InpT5 inp5) {
            return RunHttpRequest(interfaceName, methodName, deserialize, Tuple.Create(inp1, inp2, inp3, inp4, inp5));
        }
        
        public static Task<OutpT> RunHttpRequest<InpT1,InpT2,InpT3,InpT4,OutpT>(string interfaceName, string methodName, Func<string,OutpT> deserialize, InpT1 inp1, InpT2 inp2, InpT3 inp3, InpT4 inp4) {
            return RunHttpRequest(interfaceName, methodName, deserialize, Tuple.Create(inp1, inp2, inp3, inp4));
        }

        public static Task<OutpT> RunHttpRequest<InpT1,InpT2,InpT3,OutpT>(string interfaceName, string methodName, Func<string,OutpT> deserialize, InpT1 inp1, InpT2 inp2, InpT3 inp3) {
            return RunHttpRequest(interfaceName, methodName, deserialize, Tuple.Create(inp1, inp2, inp3));
        }

        public static Task<OutpT> RunHttpRequest<InpT1,InpT2,OutpT>(string interfaceName, string methodName, Func<string,OutpT> deserialize, InpT1 inp1, InpT2 inp2) {
            return RunHttpRequest(interfaceName, methodName, deserialize, Tuple.Create(inp1, inp2));
        }

        public static async Task<OutpT> RunHttpRequest<InpT,OutpT>(
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

        //RunHttpRequestReturningArray: params 5 to 0

        public static async Task<OutpT[]> RunHttpRequestReturningArray<InpT1,InpT2,InpT3,InpT4,InpT5,OutpT>(string interfaceName, string methodName, InpT1 inp1, InpT2 inp2, InpT3 inp3, InpT4 inp4, InpT5 inp5) where OutpT : new() {
            return await RunHttpRequest(interfaceName, methodName, x => JsonConvert.DeserializeObject<OutpT[]>(x), inp1, inp2, inp3, inp4, inp5);
        }

        public static async Task<OutpT[]> RunHttpRequestReturningArray<InpT1,InpT2,InpT3,InpT4,OutpT>(string interfaceName, string methodName, InpT1 inp1, InpT2 inp2, InpT3 inp3, InpT4 inp4) where OutpT : new() {
            return await RunHttpRequest(interfaceName, methodName, x => JsonConvert.DeserializeObject<OutpT[]>(x), inp1, inp2, inp3, inp4);
        }

        public static async Task<OutpT[]> RunHttpRequestReturningArray<InpT1,InpT2,InpT3,OutpT>(string interfaceName, string methodName, InpT1 inp1, InpT2 inp2, InpT3 inp3) where OutpT : new() {
            return await RunHttpRequest(interfaceName, methodName, x => JsonConvert.DeserializeObject<OutpT[]>(x), inp1, inp2, inp3);
        }

        public static async Task<OutpT[]> RunHttpRequestReturningArray<InpT1,InpT2,OutpT>(string interfaceName, string methodName, InpT1 inp1, InpT2 inp2) where OutpT : new() {
            return await RunHttpRequest(interfaceName, methodName, x => JsonConvert.DeserializeObject<OutpT[]>(x), inp1, inp2);
        }

        public static async Task<OutpT[]> RunHttpRequestReturningArray<InpT,OutpT>(string interfaceName, string methodName, InpT inp) where OutpT : new() {
            return await RunHttpRequest(interfaceName, methodName, x => JsonConvert.DeserializeObject<OutpT[]>(x), inp);
        }

        public static async Task<OutpT[]> RunHttpRequestReturningArray<OutpT>(string interfaceName, string methodName) where OutpT : new() {
            return await RunHttpRequest(interfaceName, methodName, x => JsonConvert.DeserializeObject<OutpT[]>(x));
        }
        
        //RunHttpRequestReturningPlain: params 5 to 0

        public static async Task<OutpT> RunHttpRequestReturningPlain<InpT1,InpT2,InpT3,InpT4,InpT5,OutpT>(string interfaceName, string methodName, InpT1 inp1, InpT2 inp2, InpT3 inp3, InpT4 inp4, InpT5 inp5) {
            return await RunHttpRequest(interfaceName, methodName, x => NullAwareJsonParse<OutpT>(x), inp1, inp2, inp3, inp4, inp5);
        }

        public static async Task<OutpT> RunHttpRequestReturningPlain<InpT1,InpT2,InpT3,InpT4,OutpT>(string interfaceName, string methodName, InpT1 inp1, InpT2 inp2, InpT3 inp3, InpT4 inp4) {
            return await RunHttpRequest(interfaceName, methodName, x => NullAwareJsonParse<OutpT>(x), inp1, inp2, inp3, inp4);
        }

        public static async Task<OutpT> RunHttpRequestReturningPlain<InpT1,InpT2,InpT3,OutpT>(string interfaceName, string methodName, InpT1 inp1, InpT2 inp2, InpT3 inp3) {
            return await RunHttpRequest(interfaceName, methodName, x => NullAwareJsonParse<OutpT>(x), inp1, inp2, inp3);
        }

        public static async Task<OutpT> RunHttpRequestReturningPlain<InpT1,InpT2,OutpT>(string interfaceName, string methodName, InpT1 inp1, InpT2 inp2) {
            return await RunHttpRequest(interfaceName, methodName, x => NullAwareJsonParse<OutpT>(x), inp1, inp2);
        }

		public static async Task<OutpT> RunHttpRequestReturningPlain<InpT,OutpT>(string interfaceName, string methodName, InpT inp) {
			return await RunHttpRequest(interfaceName, methodName, x => NullAwareJsonParse<OutpT>(x), inp);
		}
        
		public static async Task<OutpT> RunHttpRequestReturningPlain<OutpT>(string interfaceName, string methodName) {
			return await RunHttpRequest(interfaceName, methodName, x => NullAwareJsonParse<OutpT>(x));
		}

        //RunHttpRequestReturningAttachment: params 5 to 0

        public static Task<FileModel> RunHttpRequestReturningAttachment<InpT1,InpT2,InpT3,InpT4,InpT5>(string interfaceName, string methodName, InpT1 inp1, InpT2 inp2, InpT3 inp3, InpT4 inp4, InpT5 inp5) {
            return RunHttpRequestReturningAttachmentImpl(interfaceName, methodName, Tuple.Create(inp1, inp2, inp3, inp4, inp5));
        }

        public static Task<FileModel> RunHttpRequestReturningAttachment<InpT1,InpT2,InpT3,InpT4>(string interfaceName, string methodName, InpT1 inp1, InpT2 inp2, InpT3 inp3, InpT4 inp4) {
            return RunHttpRequestReturningAttachmentImpl(interfaceName, methodName, Tuple.Create(inp1, inp2, inp3, inp4));
        }

        public static Task<FileModel> RunHttpRequestReturningAttachment<InpT1,InpT2,InpT3>(string interfaceName, string methodName, InpT1 inp1, InpT2 inp2, InpT3 inp3) {
            return RunHttpRequestReturningAttachmentImpl(interfaceName, methodName, Tuple.Create(inp1, inp2, inp3));
        }
        
        public static Task<FileModel> RunHttpRequestReturningAttachment<InpT1,InpT2>(string interfaceName, string methodName, InpT1 inp1, InpT2 inp2) {
            return RunHttpRequestReturningAttachmentImpl(interfaceName, methodName, Tuple.Create(inp1, inp2));
        }
        
        public static Task<FileModel> RunHttpRequestReturningAttachment<InpT>(string interfaceName, string methodName, InpT inputParam) {
            return RunHttpRequestReturningAttachmentImpl(interfaceName, methodName, inputParam);
        }
        
        public static Task<FileModel> RunHttpRequestReturningAttachment(string interfaceName, string methodName) {
            return RunHttpRequestReturningAttachmentImplStr(interfaceName, methodName, "");
        }
        
        public static Task<FileModel> RunHttpRequestReturningAttachmentImpl<T>(string interfaceName, string methodName, T inputParam) {
            return RunHttpRequestReturningAttachmentImplStr(interfaceName, methodName, JSON.Stringify(inputParam));
        }

        public static Task<FileModel> RunHttpRequestReturningAttachmentImplStr(string interfaceName, string methodName, string inputParam) {
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

            if (CsrfToken != null) {
                form.AppendChild(new HTMLInputElement {
                    Type = InputType.Hidden,
                    Name = Philadelphia.Common.Model.Magics.CsrfTokenFieldName,
                    Value = CsrfToken });
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
        
        public static string BuildUrl(string interfaceName, string methodName, string inputParam) {
            var result = 
                string.Format(
                    "/{0}/{1}?{2}=", interfaceName, methodName, Magics.PostReturningFileParameterName) + 
                Window.EncodeURIComponent(inputParam);

            Logger.Debug(typeof(HttpRequester), "built GET url={0}", result);

            return result;
        }
    }
}
