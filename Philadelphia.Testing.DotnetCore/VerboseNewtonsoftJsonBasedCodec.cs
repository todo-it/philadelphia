using System;
using Newtonsoft.Json;

namespace Philadelphia.Testing.DotnetCore {
    public class VerboseNewtonsoftJsonBasedCodec : ICodec {
        public T Decode<T>(string txt) {
            try {
                return JsonConvert.DeserializeObject<T>(txt);
            } catch (Exception ex) {
                throw new Exception($"could not deserialize as type={typeof(T).FullName} rawJson={txt}", ex);
            }
        }

        public string Encode<T>(T obj) {
            return JsonConvert.SerializeObject(obj);
        }
    }
}
