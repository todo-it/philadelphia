[<AutoOpen>]
module Philadelphia.Server.Common.StdLibExtensions
open System.Collections.Generic
open System

type IDictionary<'Key, 'Value> with
    member inline d.MaybeGetValue key =
        match d.TryGetValue key with
        | false, _ -> None
        | true, v -> Some v

module Nullable =
    let map fn (x:Nullable<_>) =
        if x.HasValue 
        then fn x.Value |> Nullable<_>
        else x

    let(|Some|None|) (x:Nullable<_>) = if x.HasValue then Some x.Value else None