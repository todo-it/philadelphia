module Philadelphia.ServerSideUtils.Json

open Newtonsoft.Json

type ICodec =
    abstract Decode: string -> 'T
    abstract Encode: 'T -> string
    abstract EncodeNotIncludingNullProperties: 'T -> string
    abstract Decode: System.Type * string -> obj
    
type DefaultCodec() = 
    interface ICodec with
        member __.Decode input = JsonConvert.DeserializeObject<_> input
        member __.Encode input  = JsonConvert.SerializeObject(input)
        member __.EncodeNotIncludingNullProperties input = 
            JsonConvert.SerializeObject(input, JsonSerializerSettings(NullValueHandling = NullValueHandling.Ignore))            
        member __.Decode (t, input) = JsonConvert.DeserializeObject(input,t)
