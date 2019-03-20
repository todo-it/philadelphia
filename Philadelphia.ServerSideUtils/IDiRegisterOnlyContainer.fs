module Philadelphia.ServerSideUtils.IDiRegisterOnlyContainer

open Philadelphia.Common
// review: maybe put extension as F# style extension method and make module [<AutoOpen>]
///handy extension for F#
let registerFactoryMethod (c:IDiRegisterOnlyContainer) f s = 
    c.RegisterFactoryMethod(System.Func<_,_> f, s) |> ignore
