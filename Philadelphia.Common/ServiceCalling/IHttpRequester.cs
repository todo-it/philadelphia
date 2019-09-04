using System;
using System.Threading.Tasks;

namespace Philadelphia.Common {
    public interface IHttpRequester {
        Task<OutpT> RunHttpRequest<InpT, OutpT>(
            string interfaceName, string methodName,
            Func<string, OutpT> deserialize, 
            InpT inp);

        Task<FileModel> RunHttpRequestReturningAttachmentImplStr(
            string interfaceName, 
            string methodName,
            string inputParam);

        string BuildUrl(string interfaceName, string methodName, string inputParam);

        T DeserializeObject<T>(string input);
        string SerializeObject<T>(T input);
    }

    public static class HttpRequester
    {
        public static string CsrfToken { get; set; }

        private static T NullAwareJsonParse<T>(this IHttpRequester r, string input)
        {
            return "null".Equals(input) ? default(T) : r.DeserializeObject<T>(input);
        }

        public static Task<OutpT> RunHttpRequest<OutpT>(this IHttpRequester r,
            string interfaceName, string methodName, Func<string, OutpT> deserialize)
        {
            return r.RunHttpRequest(interfaceName, methodName, deserialize, "");
        }

        public static Task<OutpT> RunHttpRequest<InpT1, InpT2, InpT3, InpT4, InpT5, OutpT>(
            this IHttpRequester r,
            string interfaceName, string methodName, Func<string, OutpT> deserialize, InpT1 inp1, InpT2 inp2, InpT3 inp3, InpT4 inp4, InpT5 inp5)
        {
            return r.RunHttpRequest(interfaceName, methodName, deserialize, Tuple.Create(inp1, inp2, inp3, inp4, inp5));
        }

        public static Task<OutpT> RunHttpRequest<InpT1, InpT2, InpT3, InpT4, OutpT>(
            this IHttpRequester r,
            string interfaceName, string methodName, Func<string, OutpT> deserialize, InpT1 inp1, InpT2 inp2, InpT3 inp3, InpT4 inp4)
        {
            return r.RunHttpRequest(interfaceName, methodName, deserialize, Tuple.Create(inp1, inp2, inp3, inp4));
        }

        public static Task<OutpT> RunHttpRequest<InpT1, InpT2, InpT3, OutpT>(
            this IHttpRequester r,
            string interfaceName, string methodName, Func<string, OutpT> deserialize, InpT1 inp1, InpT2 inp2, InpT3 inp3)
        {
            return r.RunHttpRequest(interfaceName, methodName, deserialize, Tuple.Create(inp1, inp2, inp3));
        }

        public static Task<OutpT> RunHttpRequest<InpT1, InpT2, OutpT>(
            this IHttpRequester r,
            string interfaceName, string methodName, Func<string, OutpT> deserialize, InpT1 inp1, InpT2 inp2)
        {
            return r.RunHttpRequest(interfaceName, methodName, deserialize, Tuple.Create(inp1, inp2));
        }

        //RunHttpRequestReturningArray: params 5 to 0

        public static async Task<OutpT[]> RunHttpRequestReturningArray<InpT1, InpT2, InpT3, InpT4, InpT5, OutpT>(
            this IHttpRequester r,
            string interfaceName, string methodName, InpT1 inp1, InpT2 inp2, InpT3 inp3, InpT4 inp4, InpT5 inp5) where OutpT : new()
        {
            return await r.RunHttpRequest(interfaceName, methodName, x => r.DeserializeObject<OutpT[]>(x), inp1, inp2, inp3, inp4, inp5);
        }

        public static async Task<OutpT[]> RunHttpRequestReturningArray<InpT1, InpT2, InpT3, InpT4, OutpT>(
            this IHttpRequester r,
            string interfaceName, string methodName, InpT1 inp1, InpT2 inp2, InpT3 inp3, InpT4 inp4) where OutpT : new()
        {
            return await r.RunHttpRequest(interfaceName, methodName, x => r.DeserializeObject<OutpT[]>(x), inp1, inp2, inp3, inp4);
        }

        public static async Task<OutpT[]> RunHttpRequestReturningArray<InpT1, InpT2, InpT3, OutpT>(
            this IHttpRequester r,
            string interfaceName, string methodName, InpT1 inp1, InpT2 inp2, InpT3 inp3) where OutpT : new()
        {
            return await r.RunHttpRequest(interfaceName, methodName, x => r.DeserializeObject<OutpT[]>(x), inp1, inp2, inp3);
        }

        public static async Task<OutpT[]> RunHttpRequestReturningArray<InpT1, InpT2, OutpT>(
            this IHttpRequester r,
            string interfaceName, string methodName, InpT1 inp1, InpT2 inp2) where OutpT : new()
        {
            return await r.RunHttpRequest(interfaceName, methodName, x => r.DeserializeObject<OutpT[]>(x), inp1, inp2);
        }

        public static async Task<OutpT[]> RunHttpRequestReturningArray<InpT, OutpT>(
            this IHttpRequester r,
            string interfaceName, string methodName, InpT inp) where OutpT : new()
        {
            return await r.RunHttpRequest(interfaceName, methodName, x => r.DeserializeObject<OutpT[]>(x), inp);
        }

        public static async Task<OutpT[]> RunHttpRequestReturningArray<OutpT>(
            this IHttpRequester r,
            string interfaceName, string methodName) where OutpT : new()
        {
            return await r.RunHttpRequest(interfaceName, methodName, x => r.DeserializeObject<OutpT[]>(x));
        }

        //RunHttpRequestReturningPlain: params 5 to 0

        public static async Task<OutpT> RunHttpRequestReturningPlain<InpT1, InpT2, InpT3, InpT4, InpT5, OutpT>(
            this IHttpRequester r,
            string interfaceName, string methodName, InpT1 inp1, InpT2 inp2, InpT3 inp3, InpT4 inp4, InpT5 inp5)
        {
            return await r.RunHttpRequest(interfaceName, methodName, x => r.NullAwareJsonParse<OutpT>(x), inp1, inp2, inp3, inp4, inp5);
        }

        public static async Task<OutpT> RunHttpRequestReturningPlain<InpT1, InpT2, InpT3, InpT4, OutpT>(
            this IHttpRequester r,
            string interfaceName, string methodName, InpT1 inp1, InpT2 inp2, InpT3 inp3, InpT4 inp4)
        {
            return await r.RunHttpRequest(interfaceName, methodName, x => r.NullAwareJsonParse<OutpT>(x), inp1, inp2, inp3, inp4);
        }

        public static async Task<OutpT> RunHttpRequestReturningPlain<InpT1, InpT2, InpT3, OutpT>(
            this IHttpRequester r, 
            string interfaceName, string methodName, InpT1 inp1, InpT2 inp2, InpT3 inp3)
        {
            return await r.RunHttpRequest(interfaceName, methodName, x => r.NullAwareJsonParse<OutpT>(x), inp1, inp2, inp3);
        }

        public static async Task<OutpT> RunHttpRequestReturningPlain<InpT1, InpT2, OutpT>(
            this IHttpRequester r,
            string interfaceName, string methodName, InpT1 inp1, InpT2 inp2)
        {
            return await r.RunHttpRequest(interfaceName, methodName, x => r.NullAwareJsonParse<OutpT>(x), inp1, inp2);
        }

        public static async Task<OutpT> RunHttpRequestReturningPlain<InpT, OutpT>(
            this IHttpRequester r,
            string interfaceName, string methodName, InpT inp)
        {
            return await r.RunHttpRequest(interfaceName, methodName, x => r.NullAwareJsonParse<OutpT>(x), inp);
        }

        public static async Task<OutpT> RunHttpRequestReturningPlain<OutpT>(
            this IHttpRequester r,
            string interfaceName, string methodName)
        {
            return await r.RunHttpRequest(interfaceName, methodName, x => r.NullAwareJsonParse<OutpT>(x));
        }

        //RunHttpRequestReturningAttachment: params 5 to 0

        public static Task<FileModel> RunHttpRequestReturningAttachment<InpT1, InpT2, InpT3, InpT4, InpT5>(
            this IHttpRequester r,
            string interfaceName, string methodName, InpT1 inp1, InpT2 inp2, InpT3 inp3, InpT4 inp4, InpT5 inp5)
        {
            return r.RunHttpRequestReturningAttachmentImpl(interfaceName, methodName, Tuple.Create(inp1, inp2, inp3, inp4, inp5));
        }

        public static Task<FileModel> RunHttpRequestReturningAttachment<InpT1, InpT2, InpT3, InpT4>(
            this IHttpRequester r,
            string interfaceName, string methodName, InpT1 inp1, InpT2 inp2, InpT3 inp3, InpT4 inp4)
        {
            return r.RunHttpRequestReturningAttachmentImpl(interfaceName, methodName, Tuple.Create(inp1, inp2, inp3, inp4));
        }

        public static Task<FileModel> RunHttpRequestReturningAttachment<InpT1, InpT2, InpT3>(
            this IHttpRequester r,
            string interfaceName, string methodName, InpT1 inp1, InpT2 inp2, InpT3 inp3)
        {
            return r.RunHttpRequestReturningAttachmentImpl(interfaceName, methodName, Tuple.Create(inp1, inp2, inp3));
        }

        public static Task<FileModel> RunHttpRequestReturningAttachment<InpT1, InpT2>(
            this IHttpRequester r,
            string interfaceName, string methodName, InpT1 inp1, InpT2 inp2)
        {
            return r.RunHttpRequestReturningAttachmentImpl(interfaceName, methodName, Tuple.Create(inp1, inp2));
        }

        public static Task<FileModel> RunHttpRequestReturningAttachment<InpT>(
            this IHttpRequester r,
            string interfaceName, string methodName, InpT inputParam)
        {
            return r.RunHttpRequestReturningAttachmentImpl(interfaceName, methodName, inputParam);
        }

        public static Task<FileModel> RunHttpRequestReturningAttachment(
            this IHttpRequester r,
            string interfaceName, string methodName)
        {
            return r.RunHttpRequestReturningAttachmentImplStr(interfaceName, methodName, "");
        }

        public static Task<FileModel> RunHttpRequestReturningAttachmentImpl<T>(
            this IHttpRequester r, string interfaceName, string methodName, T inputParam)
        {
            return r.RunHttpRequestReturningAttachmentImplStr(interfaceName, methodName, r.SerializeObject(inputParam));
        }
    }

}