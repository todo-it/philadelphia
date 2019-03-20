[<AutoOpen>]
module Philadelphia.Server.Common.StdLibExtensions
open System.Collections.Generic

type IDictionary<'Key, 'Value> with
    member inline d.MaybeGetValue key =
        match d.TryGetValue key with
        | false, _ -> None
        | true, v -> Some v