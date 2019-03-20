using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bridge.Html5;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public static class XMLHttpRequestUtils {
        public static Task<ResultHolder<XMLHttpRequest>> Upload(
            string interfaceName, string methodName, IEnumerable<Bridge.Html5.File> uploaded, 
            FileUploadOperation operation, RemoteFileId toReplaceOrRemoveId, string jsonizedInput) {

            //for FormData upload see https://stackoverflow.com/questions/6133800/html5-file-api-readasbinarystring-reads-files-as-much-larger-different-than-fil/6142797#6142797
            
            var uploadData = new FormData();
            
            uploaded.ForEachI((i,x) => uploadData.Append("file"+i, x));
            uploadData.Append("operation", ((int)operation).ToString());
            uploadData.Append("toReplaceOrRemoveId", JSON.Stringify(toReplaceOrRemoveId));
            uploadData.Append("i", jsonizedInput);
            
            return Task.FromPromise<ResultHolder<XMLHttpRequest>>(
                new XMLHttpRequestImplementingIPromise("POST", $"/{interfaceName}/{methodName}", uploadData),
                (Func<ResultHolder<XMLHttpRequest>, ResultHolder<XMLHttpRequest>>)(x => x));
        }
    }
}
