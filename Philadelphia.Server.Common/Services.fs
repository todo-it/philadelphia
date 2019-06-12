namespace Philadelphia.Server.Common

open System.Collections.Generic
open Philadelphia.Common
open System.Threading.Tasks
open System
open System.Linq
open System.Reflection
open Newtonsoft.Json
open System.Threading
open System.IO
open System.Text
open System.Net.Http

type HttpMethodVerb = 
|POST
|GET

//TODO rename to ImmediateReturnService
type ImmediateReturnPostService = {
    Method:HttpMethodVerb
    ReturnsFile:bool
    IsUpload:bool
    Url:string
    HasParameters:bool
    InputDeserialization:Func<string, obj[]>
    ServiceInvoker:Func<IDiResolveReleaseOnlyContainer, obj[], Async<ServiceReply>> }

type ServerSentEventService = {
    Url:string
    Inp:Type
    Outp:Type
    Di:IDiRegisterOnlyContainer //FIXME should not be passed this way
    FilterFacBuilder:(IDiResolveReleaseOnlyContainer->obj) * MethodInfo
}

type Msg<'TMsg> =
|Publish of 'TMsg
|Subscribe of onDispose:Async<unit> * Stream * Func<'TMsg,bool>
|Unsubscribe of Stream

///simplifies process: handles serialization, deserialization, ILifeTimeFilter updates
type SimpleSubscription = {
    Subscribe: IDiResolveReleaseOnlyContainer->Stream->string->Async<FilterReply option>
    Unsubscribe: IDiResolveReleaseOnlyContainer->Stream->unit
}

module Async =
    let CreateResult x = async {return x}

type LifestyleFilteredResult<'TMsg,'TClientCtx> =
|Rejected of FilterReply
|Accepted of logicalFilterFac:('TClientCtx->Func<'TMsg,bool>) * onDisposeFac:Async<unit>

type Subscription<'TMsg,'TClientCtx> = {
    Mbox : MailboxProcessor<Msg<'TMsg>>
    Send : 'TMsg->unit
    Subscribe: Async<unit>->Stream->Func<'TMsg,bool>->unit
    Unsubscribe: Stream->unit
    StringToClientCtx:string->'TClientCtx
    HandleConnection: IDiResolveReleaseOnlyContainer->'TClientCtx->Async<LifestyleFilteredResult<'TMsg,'TClientCtx>>
}
with
    member self.SendMessage msg = self.Send msg
    member x.SimpleSubscription : SimpleSubscription = {
        Subscribe = fun di cl prms -> async {
            let ctx = prms |> x.StringToClientCtx
            let! result = x.HandleConnection di ctx 

            return
                match result with
                |Accepted(logicalFilterFac, onDispose) ->
                    let logicalFilter = ctx |> logicalFilterFac
                    if logicalFilter = null
                    then 
                        {
                            FilterReply.HttpStatus = 400
                            ContentType = "text/plain"
                            Content = 
                                "not permitted - rejected by logical filter" |> Encoding.UTF8.GetBytes }
                        |> Some 
                    else 
                        x.Subscribe onDispose cl logicalFilter
                        None
                |Rejected(filteredOut) -> filteredOut |> Some }

        Unsubscribe = fun di stream -> x.Unsubscribe stream
    }

type ClientState<'TMsg> = {
    Filter : Func<'TMsg,bool>
    OnDispose: Async<unit>
}

type MsgBoxState<'TMsg> = {    
    Clients : IDictionary<Stream,ClientState<'TMsg>>    
}

module ServerPush =
    type internal Iface = interface end

    let writeToClient (onError:Stream->exn->unit) (cl:Stream) (msg:string) : unit = 
        let worker = 
            async {                
                let msg = Encoding.UTF8.GetBytes("data: "+msg+"\n\n")
                do! cl.WriteAsync(msg, 0, msg.Length) |> Async.AwaitTask
                do! cl.FlushAsync() |> Async.AwaitTask                    
                return ()
            } 
            
        async {
            let! result = worker |> Async.Catch
            match result with
            |Choice1Of2 _ -> 
                Logger.Debug(typeof<Iface>, "successfully sent message to client")
                return ()
            |Choice2Of2 ex -> 
                Logger.Debug(typeof<Iface>, "failed to send to client - unsubscribing")
                onError cl ex
                return ()
        }
        |> Async.Start
    
    let mboxBuilder<'TMsg> messageSerializer =
        new MailboxProcessor<Msg<'TMsg>>(fun inbox ->
            let log x = Logger.Debug(typeof<MailboxProcessor<Msg<'TMsg>>>, x )
            let rec loop (state:MsgBoxState<'TMsg>) =
                let unsubscriber cl _ = cl |> Unsubscribe |> inbox.Post
                async { 
                    log "mbox waiting..."
                    let! msg = inbox.Receive()
                
                    let! state =
                        match msg with
                        |Publish(msgContent) ->
                            state.Clients
                            |> Seq.iter(fun cl ->
                                let state = cl.Value
                                if (state.Filter.Invoke(msgContent))
                                then
                                    log "client is interested - filter accepted"
                                    writeToClient unsubscriber cl.Key (messageSerializer msgContent)
                                else
                                    log "client is not interested - filter rejected" )

                            state |> Async.CreateResult
                        |Subscribe(onDispose,stream,filterImpl) ->
                            log "mbox subscribing client"
                            
                            state.Clients.Add(
                                stream, 
                                    {
                                        ClientState.Filter = filterImpl
                                        OnDispose = onDispose
                                    })
                            state |> Async.CreateResult
                        |Unsubscribe(stream) ->
                            log "mbox unsubscribing client"
                            
                            match state.Clients.TryGetValue stream with
                            |true, clState ->
                                state.Clients.Remove(stream) |> ignore
                                    
                                async {
                                    let! _ = clState.OnDispose
                                    return state
                                }                                    
                            |_ -> 
                                log "mbox unsubscribing failed - no such client"
                                state |> Async.CreateResult
                            
                    sprintf "mbox clients count after %d" state.Clients.Count |> log

                    return! loop state }

            {
                MsgBoxState.Clients = Dictionary<_,_>()
                }
            |> loop )

module ServerPushReg =
    let register<'TMsg,'TSubsParms> url (filterBuilder:(IDiResolveReleaseOnlyContainer->obj) * MethodInfo) =
        let mbox = 
            ServerPush.mboxBuilder<'TMsg> 
                (fun (x:'TMsg) -> Newtonsoft.Json.JsonConvert.SerializeObject(x))

        mbox.Start()
        
        let result = 
            {
                Subscription.Mbox = mbox
                HandleConnection = (fun di ctx -> async {
                    let serviceInstProv, method = filterBuilder
                    let serviceInst = serviceInstProv di

                    let lifetimeFilter:ILifetimeFilter = di.Resolve()

                    let! lifetimeFilterRes =
                        lifetimeFilter.OnConnectionBeforeHandler(
                            di, url, serviceInst, method, [| ctx |], ResourceType.ServerSentEventListener)
                        |> Async.AwaitTask

                    return
                        match lifetimeFilterRes.MaybeFiltered with
                        |Some reply -> LifestyleFilteredResult.Rejected reply
                        |None ->                            
                            let logicalFilterFac (z:'TSubsParms) =
                                let filter = method.Invoke(serviceInst, [| z |] ) 
                                filter :?> System.Func<'TMsg,bool>
                    
                            let onDispose = async {
                                do!
                                    lifetimeFilter.OnConnectionAfterHandler(
                                        lifetimeFilterRes.ConnectionCtx, di, null)
                                    |> Async.AwaitTask 
                                return ()}

                            LifestyleFilteredResult.Accepted(logicalFilterFac, onDispose) })
                Send = (fun (x:'TMsg) -> 
                    x 
                    |> Publish 
                    |> mbox.Post)
                Subscribe = (fun connCtx (cl:Stream) filter ->
                    (connCtx, cl, filter)
                    |> Subscribe
                    |> mbox.Post )
                Unsubscribe = (fun (cl:Stream) -> 
                    cl
                    |> Unsubscribe
                    |> mbox.Post )
                StringToClientCtx = fun prms -> Newtonsoft.Json.JsonConvert.DeserializeObject<'TSubsParms>(prms)
            }

        result
   
type ServerSentRegistrator =
    static member RegisterPushListener<'O,'I> url (di:IDiRegisterOnlyContainer) filterFac =
        let subscr = ServerPushReg.register<'O,'I> url filterFac
                
        di.RegisterFactoryMethod<Subscription<'O,'I>> (
            (fun _ -> subscr),
            LifeStyle.Singleton)

        subscr.SimpleSubscription

    static member Register (descr:ServerSentEventService) =
        let gmth = typeof<ServerSentRegistrator>.GetMethod "RegisterPushListener"
        let mth = gmth.MakeGenericMethod [| descr.Outp; descr.Inp |]
        let res = mth.Invoke(null, [| descr.Url; descr.Di; descr.FilterFacBuilder |]) 
        res :?> SimpleSubscription
             
module Services =
    type internal Iface = interface end

    let registerRequestDelegation serviceType m url registerImmediateRequest =
        let returnsFile = (m:MethodInfo).ReturnType = typeof<Task<FileModel>>
        let prms = m.GetParameters().ToList()
        let hasParameters = prms.Count <> 0
        let isUploadMode = prms.Count > 0 && prms.[0].ParameterType = typeof<Philadelphia.Common.UploadInfo>

        let inputDeserialization : (string->obj[]) =
            if hasParameters 
            then (fun x -> 
                let prms = if isUploadMode then prms.Skip(1).ToList() else prms
                match prms.Count with
                |0 -> [| |]
                |1 -> [| JsonConvert.DeserializeObject(x, prms.First().ParameterType) |]
                |2 -> 
                    // review: there are Fsharp.Core tools to do things like this: Microsoft.FSharp.Reflection
                    // FSharpType.MakeTupleType
                    // FSharpValue.GetTupleFields
                    let tuple = typedefof<System.Tuple<_,_>>
                    let fullTuple= 
                        tuple.MakeGenericType
                            [| 
                                prms.[0].ParameterType
                                prms.[1].ParameterType 
                            |] 
                                            
                    let tupleValue = JsonConvert.DeserializeObject(x, fullTuple)

                    [| 
                        fullTuple.GetProperty("Item1").GetValue(tupleValue)
                        fullTuple.GetProperty("Item2").GetValue(tupleValue)
                    |]
                |3 -> 
                    let tuple = typedefof<System.Tuple<_,_,_>>
                    let fullTuple= 
                        tuple.MakeGenericType
                            [| 
                                prms.[0].ParameterType
                                prms.[1].ParameterType 
                                prms.[2].ParameterType 
                            |]
                    
                    let tupleValue = JsonConvert.DeserializeObject(x, fullTuple)

                    [| 
                        fullTuple.GetProperty("Item1").GetValue(tupleValue) 
                        fullTuple.GetProperty("Item2").GetValue(tupleValue)
                        fullTuple.GetProperty("Item3").GetValue(tupleValue)
                    |]
                |4 -> 
                    let tuple = typedefof<System.Tuple<_,_,_,_>>
                    let fullTuple= 
                        tuple.MakeGenericType
                            [| 
                                prms.[0].ParameterType
                                prms.[1].ParameterType 
                                prms.[2].ParameterType 
                                prms.[3].ParameterType 
                            |]
                    
                    let tupleValue = JsonConvert.DeserializeObject(x, fullTuple)

                    [| 
                        fullTuple.GetProperty("Item1").GetValue(tupleValue) 
                        fullTuple.GetProperty("Item2").GetValue(tupleValue)
                        fullTuple.GetProperty("Item3").GetValue(tupleValue)
                        fullTuple.GetProperty("Item4").GetValue(tupleValue)
                    |]
                |5 -> 
                    let tuple = typedefof<System.Tuple<_,_,_,_,_>>
                    let fullTuple= 
                        tuple.MakeGenericType
                            [| 
                                prms.[0].ParameterType
                                prms.[1].ParameterType 
                                prms.[2].ParameterType 
                                prms.[3].ParameterType 
                                prms.[4].ParameterType 
                            |]
                    
                    let tupleValue = JsonConvert.DeserializeObject(x, fullTuple)

                    [| 
                        fullTuple.GetProperty("Item1").GetValue(tupleValue) 
                        fullTuple.GetProperty("Item2").GetValue(tupleValue)
                        fullTuple.GetProperty("Item3").GetValue(tupleValue)
                        fullTuple.GetProperty("Item4").GetValue(tupleValue)
                        fullTuple.GetProperty("Item5").GetValue(tupleValue)
                    |]
                |x -> failwithf "not supported %d parameter count" x
                )
            else (fun _ -> [| |])

        let serviceInvoker (di:IDiResolveReleaseOnlyContainer) x = async {
            let serviceImpl = di.Resolve(serviceType) 
            
            let connFilter:ILifetimeFilter = di.Resolve()

            let! lifetimeFilterRes =
                connFilter.OnConnectionBeforeHandler(di, url, serviceImpl, m, x, ResourceType.RegularPostService)
                |> Async.AwaitTask

            return
                match lifetimeFilterRes.MaybeFiltered with
                |Some filterReply -> 
                    filterReply |> Task.FromResult |> ServiceReply.Filtered
                |None ->
                    let connCtx = lifetimeFilterRes.ConnectionCtx

                    async {
                        let! invoked = 
                            async {
                                let tsk =
                                    m.Invoke(
                                        serviceImpl,
                                        if hasParameters then x else [||] )

                                return! 
                                    tsk
                                    |> ObjectUtil.AwaitForUnknownTask
                                    |> Async.AwaitTask
                            }
                            |> Async.Catch

                        return!
                            match invoked with
                            |Choice1Of2(succ) -> 
                                async {
                                    let! _ =
                                        connFilter.OnConnectionAfterHandler(connCtx, di, null)
                                        |> Async.AwaitTask

                                    return Result.Ok succ
                                }
                            |Choice2Of2(ex) ->
                                async {
                                    let! _ =
                                        connFilter.OnConnectionAfterHandler(connCtx, di, ex)
                                        |> Async.AwaitTask

                                    return Result.Error ex
                                }
                    } 
                    |> ServiceReply.Invoked
            }
        
        let log x = Logger.Debug(typeof<Iface>, x)
        let build method =
            sprintf "registering %O request resource %A" method url |> log
            {
                ImmediateReturnPostService.ReturnsFile = returnsFile
                IsUpload = isUploadMode
                Url = url
                HasParameters = hasParameters
                InputDeserialization = System.Func<_,_> inputDeserialization
                ServiceInvoker = System.Func<_,_,_> serviceInvoker
                Method = method
            }
        
        HttpMethodVerb.POST |> build |> registerImmediateRequest

        do if returnsFile then HttpMethodVerb.GET |> build |> registerImmediateRequest
                
    let registerServices servicesToImpl di registerImmediateReturnService registerServSentEventService
            (postRegistr:string->SimpleSubscription->unit) =

        let log x = Logger.Debug(typeof<Iface>, x)
        let pushMethodRetType = typedefof<System.Func<_,_>>
        let immediateResultMethodRetType = typedefof<System.Threading.Tasks.Task<_>>

        servicesToImpl
        |> Seq.iter (fun (x:ContractToImplementation) -> 
            let intf = x.Contract;
            let intfName = intf.FullName;

            x.Contract.GetMethods()
            |> Array.iter (fun method ->
                let url = sprintf "/%s/%s" intfName method.Name

                let rt = method.ReturnType
                match rt.IsGenericType, rt.GetGenericTypeDefinition() with
                |true, z when z = pushMethodRetType -> 
                    let msgType = rt.GetGenericArguments().[0]
                    let clCtxType = method.GetParameters().[0].ParameterType
                    
                    sprintf "registering server sent event request resource %A" url |> log
                    
                    let subscr =                         
                        {
                            ServerSentEventService.Inp = clCtxType
                            Url = url
                            Outp = msgType
                            Di = di
                            FilterFacBuilder = (fun di -> di.Resolve(x.Contract)), method
                        }
                        |> registerServSentEventService
                        
                    postRegistr url subscr

                |true, z when z = immediateResultMethodRetType -> 
                    registerRequestDelegation intf method url registerImmediateReturnService
                |_ -> 
                    failwithf "neither immediate result nor push service. Expected Task<T> or Func<V,bool> respectively. Wrong method in class %s method %s" intf.FullName method.Name ))
