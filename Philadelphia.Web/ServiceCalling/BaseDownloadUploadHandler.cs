using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bridge.Html5;
using Newtonsoft.Json;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public abstract class BaseDownloadUploadHandler {
        private readonly Func<string> _input;
        private readonly string _interfaceName;
        private readonly string _getterMethod;
        private readonly string _setterMethod;
        private readonly Func<RemoteFileId, string> _downloadSer;
        
        public Func<IEnumerable<Bridge.Html5.File>, FileUploadOperation, RemoteFileId, Task<RemoteFileId[]>> UploadOrNull { 
            get {
                if (_setterMethod != null) {
                    return Upload;
                }
                
                return null;
            }}
        
        public Func<RemoteFileId, Task<FileModel>> DownloadOrNull { 
            get {
                if (_getterMethod != null) {
                    return Download;
                }
                
                return null;
            }}
        
        public Func<RemoteFileId, string> DownloadUrlOrNull { 
            get {
                if (_getterMethod != null) {
                    return BuildDownloadUrl;
                }
                
                return null;
            }}

        protected BaseDownloadUploadHandler(
                Func<string> input, string interfaceName, string getterMethod, string setterMethod, 
                Func<RemoteFileId,string> downloadSer) {

            _input = input;
            _interfaceName = interfaceName;
            _getterMethod = getterMethod;
            _setterMethod = setterMethod;
            _downloadSer = downloadSer;
        }

        public Task<FileModel> Download(RemoteFileId id) {
            return HttpRequester.RunHttpRequestReturningAttachmentImplStr(
                _interfaceName, _getterMethod, _downloadSer(id));
        }

        public string BuildDownloadUrl(RemoteFileId id) {
            return HttpRequester.BuildUrl(_interfaceName, _getterMethod, _downloadSer(id));
        }

        public async Task<RemoteFileId[]> Upload(
            IEnumerable<Bridge.Html5.File> uploaded, FileUploadOperation operation, RemoteFileId toReplaceOrRemoveId) {

            var input = _input?.Invoke();
            var res = await XMLHttpRequestUtils.Upload(
                _interfaceName, _setterMethod, uploaded, operation, toReplaceOrRemoveId, input);
            
            if (!res.Success) {
                throw new Exception(res.Result.ResponseText);    
            }
            
            var output = res.Result.Response;
            
            Logger.Debug(GetType(), "upload success file id json is {0}", output);
            return JsonConvert.DeserializeObject<RemoteFileId[]>(
                BridgeObjectUtil.NoOpCast<string>(output));
        }
    }
}
