namespace Philadelphia.Server.Common

open System
open System.Collections

type Compression =
|Disabled = 0
|Http = 1
|HttpAndHttps = 2

// review: use immutable record type instead (just make sure it is C# friendly)
type ServerSettings() =
    ///read before you decide to enable compression for HTTPS https://en.wikipedia.org/wiki/BREACH 
    member val ResponseCompression = Compression.Http with get, set
    member val CustomStaticResourceDirs = null:string seq with get, set
    member val MaxContentLengthChars = Nullable<int64>() with get, set
    member val MaxContentBoundaryChars = Nullable<int>() with get, set
    member val MaxContentBodyChars = Nullable<int64>() with get, set
    member val ServerSideEventSubscribentMaxLifeSeconds = Nullable<int>() with get, set
    member val CustomStaticResourceFileName = null:string with get, set

    ///compatible with String.Format(template, url)
    member val CustomUnsupportedUrlResponseTemplate = null:string with get, set
    static member CreateDefault() = ServerSettings()
