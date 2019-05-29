namespace Philadelphia.Server.Common

open System
open System.Collections.Generic
open System.Linq
open Philadelphia.Common
open System.Reflection
open System.Threading.Tasks
open System

type ResourceType = 
|Unsupported=0
|RegularPostService=1
|StaticResource=2
|ServerSentEventListener=3

type FilterReply = {
    HttpStatus:int
    ContentType:string
    Content:byte[]
}
    with
        member self.IsOk = self.HttpStatus >= 200 && self.HttpStatus < 400
        static member CreateTextError t = 
            {
                FilterReply.HttpStatus = 400
                ContentType = "text/plain"
                Content = (t:string) |> System.Text.Encoding.UTF8.GetBytes
            }

type ConnectionCtx = obj

type ServiceReply =
|Filtered of Task<FilterReply>
|Invoked of Async<Result<obj,exn>>

type ConnectionAction private (reply:FilterReply option, connectionCtx:ConnectionCtx) =
    member __.IsFiltered = 
        match reply with
        |Some _ -> true
        |_ -> false

    member __.MaybeFiltered = reply
    member __.ConnectionCtx = 
        match reply with
        |Some _ -> failwith "connection filtered thus no connection context is not available"
        |_ -> connectionCtx

    static member CreateFilteredOut(reply) = ConnectionAction(Some reply, null)
    static member CreateNonFiltered(connCtx) = ConnectionAction(None, connCtx)

///for handling AOP aspects such as blocking, replacing connections, dbtransactions, auditing, logging etc
[<AllowNullLiteral>]
type ILifetimeFilter =
    abstract member OnServerStarted : di:IDiResolveReleaseOnlyContainer->Task

    ///maybe replace service call(filter it out). Returns Task instead of Async to be C# friendly
    abstract member OnConnectionBeforeHandler : 
        di:IDiResolveReleaseOnlyContainer * url:string * serviceInstance:obj * m:MethodInfo * ResourceType->Task<ConnectionAction>
    
    ///not invoked for static resources.
    ///not invoked when OnConnectionBeforeHandler returned filter. 
    ///null Exception means success.
    abstract member OnConnectionAfterHandler : connectionCtx:ConnectionCtx * di:IDiResolveReleaseOnlyContainer * exOrNull:Exception->Task

type NullLifetimeFilter() =
    interface ILifetimeFilter with
        member __.OnServerStarted di = Task.CompletedTask

        member __.OnConnectionBeforeHandler(di, url, serviceInstance,m,resType) = 
            null
            |> ConnectionAction.CreateNonFiltered
            |> System.Threading.Tasks.Task.FromResult

        member __.OnConnectionAfterHandler(connCtx, di, exOrNull) = Task.CompletedTask
    static member val Instance = NullLifetimeFilter() :> ILifetimeFilter

type ContractToImplementation(contract, implementation) =
    member __.Contract with get() = contract:Type
    member __.Implementation with get() = implementation:Type

type IClientConnectionInfoConnectionIdProvider =
    abstract member Provide: unit->string 

type ServicesRegistry(services) =
    let servicesLst = (services:IEnumerable<ContractToImplementation>).ToList()

    member __.Services with get() = servicesLst

type StaticFileResource() =
    member val MimeType:string = null with get,set
    member val FileSystemPath:string = null with get,set
    
    override x.ToString () = sprintf "<StaticFileResource MimeType=%s FileSystemPath%s>" x.MimeType x.FileSystemPath

/// <summary>
/// either precise info Url+FileSystemPath or Dir+FilePattern
/// </summary>
type StaticResourceItem() =
    member val MimeType:string = null with get,set
    member val Url:string = null with get,set
    member val FileSystemPath:string = null with get,set
    member val Dir:string = null with get,set
    member val FilePattern:string = null with get,set

    member x.IsBroken =
        x.MimeType = null || 
        x.FileSystemPath = null && x.Url = null && (x.Dir = null || x.FilePattern = null) ||
        (x.FileSystemPath = null || x.Url = null) && x.Dir = null && x.FilePattern = null

    override x.ToString () = sprintf "<StaticResourceItem Dir=%s Pattern=%s Url=%s MimeType=%s FileSystemPath=%s>" x.Dir x.FilePattern x.Url x.MimeType x.FileSystemPath
