namespace Philadelphia.Server.ForAspNetCore

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Primitives
open System.Threading.Tasks
open System.Collections.Generic
open System.IO
open System.Text
open System.Threading
open Newtonsoft.Json
open Philadelphia.Common
open Philadelphia.Server.Common
open System.Reflection
open System.Text.RegularExpressions
open Microsoft.Net.Http.Headers
open Microsoft.AspNetCore.WebUtilities
open System.Runtime.InteropServices
open Philadelphia.Common.Model

type ContDispFile =
|NonFileField of name:string
|FileField of fileName:string

type HttpMethodAndUrl = {
    Verb : HttpMethodVerb
    Url : string
}

module HttpMethodVerb =
    let fromHttpContext (ctx:HttpContext) =
        match ctx.Request.Method.ToUpper() with
        |"GET" -> HttpMethodVerb.GET
        |"POST" -> HttpMethodVerb.POST
        |_ -> failwithf "unsupported verb"


[<AutoOpen>]
module Exts =
    type internal Iface = interface end

    type HttpRequest with
        member this.IsMultiPart () = this.ContentType.StartsWith("multipart/")

    let someWhenNotEmptyOrNull x = if String.IsNullOrEmpty(x) then None else Some x
    let log x = Logger.Debug(typeof<Iface>, x)

    type MultipartSection with
        member this.SanitizedEncoding =
            match MediaTypeHeaderValue.TryParse(StringSegment.op_Implicit this.ContentType) with
            |false, _ -> Encoding.UTF8
            |true, x when x.Encoding = Encoding.UTF7 -> Encoding.UTF8
            |true, x when x.Encoding <> null -> x.Encoding
            |true, _ -> Encoding.UTF8

        member this.FieldMaybe =            
            match ContentDispositionHeaderValue.TryParse(StringSegment.op_Implicit this.ContentDisposition) with
            |true, x when x <> null && "form-data".Equals(x.DispositionType.Value) -> 
                match someWhenNotEmptyOrNull x.Name.Value, someWhenNotEmptyOrNull x.FileName.Value, someWhenNotEmptyOrNull x.FileNameStar.Value with
                |_, Some x, _ -> x |> ContDispFile.FileField |> Some
                |_, _, Some x -> x |> ContDispFile.FileField |> Some
                |Some x, _, _ -> x |> ContDispFile.NonFileField |> Some
                |_ -> 
                    log "strange name&filename ContentDisposition skipped"
                    None
            |true, _ -> 
                log "unknown/unparsable ContentDisposition"
                None
            |_ -> 
                log "no ContentDisposition"
                None
                
    let rec parseFormData (reader:MultipartReader) files nonfiles =
        async {
            let! section = reader.ReadNextSectionAsync() |> Async.AwaitTask
            if section = null 
            then return files, nonfiles
            else
                let! files, nonfiles =                                         
                    match section.FieldMaybe with
                    |None -> 
                        async {
                            return files, nonfiles 
                        }
                    |Some(ContDispFile.FileField(fileName)) ->
                        async {
                            use ms = new MemoryStream()
                            do! section.Body.CopyToAsync(ms) |> Async.AwaitTask
                            let content = ms.ToArray()
                            let getContent () =  content                                            
                            return (FileModel.CreateUpload(fileName, "", Func<_> getContent)::files), nonfiles 
                        }
                        
                    |Some(ContDispFile.NonFileField(name)) -> 
                        let encoding = section.SanitizedEncoding
                        async {
                            use rdr = new StreamReader(section.Body, encoding, true, 1024, true)
                            let! content = rdr.ReadToEndAsync() |> Async.AwaitTask
                            return files, ((name, content)::nonfiles)
                        }
                                                   
                return! parseFormData reader files nonfiles
        }

type BaseStartup(
                additionalConfigureServices:Action<IServiceCollection>, 
                assemblies:IEnumerable<Assembly>, sett:ServerSettings) =

    let assemblies = List.ofSeq assemblies
    do
        if assemblies.Length <> (List.distinct assemblies |> List.length)
        then
            let asmList = 
                assemblies
                |> Seq.map(fun a -> sprintf "- %s @ %s" a.FullName a.CodeBase)
                |> String.concat "\n"
            failwithf "Duplicated assemblies detected. Full assembly list below:\n%s" asmList
            
    let log x = Logger.Debug(typeof<BaseStartup>, x)
    let logf msg = Printf.kprintf log msg
    let serverSideEventSubscribentMaxLifeSeconds = 
        sett.ServerSideEventSubscribentMaxLifeSeconds |> Option.ofNullable |> Option.defaultValue (30*60)
    let maxContentLengthChars = 
        sett.MaxContentLengthChars |> Option.ofNullable |> Option.defaultValue 40000000L 
    let maxContentBodyChars = 
        sett.MaxContentBodyChars |> Option.ofNullable |> Option.defaultValue 38000000L
    let maxContentBoundaryChars = 
        sett.MaxContentBoundaryChars |> Option.ofNullable |> Option.defaultValue 100
    let statics = Dictionary<string,StaticFileResource>()
    let servSntEvListeners = Dictionary<string,SimpleSubscription>()
    let defaultStaticResourcesDir = 
        System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof<BaseStartup>.Assembly.Location), "../../..")   
    let services = Dictionary<HttpMethodAndUrl, Func<HttpContext, Task>>()
        
    let initializeConnectionInfo (target:ClientConnectionInfo) (ctx:HttpContext) =
        let tzCode =
            match ctx.Request.Headers.TryGetValue Magics.TimeZoneCodeFieldName with 
            |true, v -> v.Item 0
            |_ -> null

        let tzOffset =
            match ctx.Request.Headers.TryGetValue Magics.TimeZoneOffsetFieldName with
            |true, v -> v.Item 0 |> int |> Nullable
            |_ -> Nullable()

        target.Initialize(
            (fun x -> 
                match ctx.Request.Cookies.TryGetValue x with
                |true, v -> v
                |_ -> null),
            (fun x -> 
                if Object.ReferenceEquals(null, x.Value) 
                then ctx.Response.Cookies.Delete(x.Name)
                else
                    let o = 
                        CookieOptions(
                            HttpOnly = x.HttpOnly,
                            Secure = x.Secure,
                            MaxAge = x.MaxAge,
                            Path = x.Path,
                            Expires = x.Expires)
                    
                    ctx.Response.Cookies.Append(x.Name, x.Value, o)),
            ctx.Connection.RemoteIpAddress.ToString(), 
            tzCode,
            tzOffset)

    let populateCsrfMaybe (target:ClientConnectionInfo) provider =
        match provider () with
        |Some x -> target.InitializeCsrfToken(x)
        |_ -> ()
            
    let buildUploadInfo files nonfiles =
        let result = UploadInfo()
                            
        result.Files <- files |> Array.ofList

        result.OperationType <-
            nonfiles 
            |> Map.tryFind "operation"
            |> function
            |Some (x:string) -> System.Convert.ToInt32(x) |> enum<FileUploadOperation>
            |None -> failwith "missed 'operation' in request"

        result.ToReplaceOrRemoveId <-
            nonfiles 
            |> Map.tryFind "toReplaceOrRemoveId"
            |> function
            |Some x -> JsonConvert.DeserializeObject<RemoteFileId>(x)
            |None -> failwith "missed 'toReplaceOrRemoveId' in request"
        result

    //upload processing is based on https://dotnetcoretutorials.com/2017/03/12/uploading-files-asp-net-core/
    let registerPostRequest (descr:ImmediateReturnPostService) =            
        let impl (ctx:HttpContext) : Task =
            let di = 
                ServiceProviderAdapterAsDiContainer(ctx.RequestServices) 
                :> IDiResolveReleaseOnlyContainer
            
            let reqParamTryGet name =
                match descr.Method with
                |HttpMethodVerb.POST -> ctx.Request.Form.TryGetValue name
                |HttpMethodVerb.GET -> ctx.Request.Query.TryGetValue name
                
            let reqParamGet name =
                match descr.Method with
                |HttpMethodVerb.POST -> ctx.Request.Form.[name]
                |HttpMethodVerb.GET -> ctx.Request.Query.[name]

            async {
                let! postDeserialization, inputAsJson =
                    match descr with
                    |{ImmediateReturnPostService.IsUpload=true} ->
                        do  if ctx.Request.ContentLength.Value > maxContentLengthChars 
                            then failwith "request's contentlength is too large"

                        do  if ctx.Request.IsMultiPart() |> not 
                            then failwith "request is not expected multipart"
                    
                        let mediaTypeHeader = 
                            ctx.Request.ContentType 
                            |> StringSegment.op_Implicit 
                            |> MediaTypeHeaderValue.Parse
                    
                        let contentBoundary = 
                            (mediaTypeHeader.Boundary |> HeaderUtilities.RemoveQuotes).Value
                    
                        do  if String.IsNullOrWhiteSpace contentBoundary
                            then failwith "contentboundary is empty or missing"
                        
                        do  if contentBoundary.Length > maxContentBoundaryChars
                            then failwith "contentboundary is too long"
                        
                        let reader = MultipartReader(contentBoundary, ctx.Request.Body)
                        reader.BodyLengthLimit <- Nullable<_> maxContentBodyChars
                         
                        async {
                            let! files, nonfiles = parseFormData reader [] []
                            let nonfiles = Map.ofList nonfiles

                            populateCsrfMaybe (di.Resolve<_>()) (fun () ->
                                let name = Magics.CsrfTokenFieldName
                                match Map.tryFind name nonfiles with
                                |Some x -> 
                                    log "found CSRF token in POST fields(in upload)"
                                    Some x
                                |_ -> None)

                            let inputAsJson =                                 
                                nonfiles 
                                |> Map.tryFind "i"
                                |> function
                                |Some x -> x
                                |None -> failwith "missed 'i' in request"

                            let uploadInfo = buildUploadInfo files nonfiles
                            let postDeserialization input =                                
                                [ [| uploadInfo :> obj |]; input] |> Seq.ofList |> Array.concat
                                
                            return postDeserialization, inputAsJson
                        }
                        
                    |{ImmediateReturnPostService.HasParameters=false} -> 
                        async {
                            return id, ""
                        }
                    |{ImmediateReturnPostService.ReturnsFile=true} ->
                        //FormPost into iframe - doesn't contain request header field
                        populateCsrfMaybe (di.Resolve<_>()) (fun () ->
                            let name = Magics.CsrfTokenFieldName
                            match reqParamTryGet name with
                            |true, x when x.Count > 0 -> 
                                log "found CSRF token in POST fields(in download)"
                                x.Item(0) |> Some
                            |_ -> None)
                        async {
                            return id, (reqParamGet "i").ToString()
                        }                        
                    |_ ->
                        async {
                            use reader = new StreamReader(ctx.Request.Body)
                            let! prm = reader.ReadToEndAsync() |> Async.AwaitTask
                            return id, prm
                        }                        
                
                let input = inputAsJson |> descr.InputDeserialization.Invoke
                let input = postDeserialization input
                    
                let! serviceOutcome = 
                    async {
                        return! descr.ServiceInvoker.Invoke(di, input)
                    }
                
                let! filterReply, serviceResult = 
                    match serviceOutcome with
                    |ServiceReply.Filtered reply -> 
                        async {
                            let! reply = reply |> Async.AwaitTask
                            log "service call filtered out"
                            return Some reply, None
                        }
                    |ServiceReply.Invoked(serviceResult) -> 
                        async {
                            let! serviceResult = serviceResult
                            log "service call not filtered out"
                            return None, Some serviceResult
                        }
                    
                let successHttpStatus = 200
                let errorHttpStatus = 400                
                let errorContentType = "text/plain"

                let textBasedError (x:string) = 
                    Result.Error(errorHttpStatus, errorContentType, x |> Encoding.UTF8.GetBytes) 

                let bytesToSend =
                    match filterReply, serviceResult, descr.ReturnsFile with
                    |Some filterReply, _, _ when filterReply.IsOk ->
                        Result.Ok(filterReply.HttpStatus, filterReply.ContentType, filterReply.Content, None)
                    |Some filterReply, _, _ ->
                        Result.Error(filterReply.HttpStatus, filterReply.ContentType, filterReply.Content)
                    |_, Some(Result.Ok(serviceResponse)), true ->                        
                        match box serviceResponse with
                        | :? FileModel as fileModel when not <| fileModel.IsAttachment ->
                            Result.Ok (successHttpStatus, fileModel.MimeType, fileModel.Content, None)
                        | :? FileModel as fileModel ->
                            Result.Ok (successHttpStatus, fileModel.MimeType, fileModel.Content, Some fileModel.FileName)
                        |_ ->
                            textBasedError ("expected FileModel but received something else")
                    |_, Some(Result.Ok(serviceResponse)), false ->
                        Result.Ok(
                            successHttpStatus,
                            "application/json; charset=utf-8",
                            serviceResponse 
                            |> JsonConvert.SerializeObject
                            |> Encoding.UTF8.GetBytes, 
                            None)
                    |_, Some(Result.Error(:? TargetInvocationException as ex)), _ ->
                        textBasedError (ex.InnerException.Message+"\n"+ex.InnerException.ToString())
                    |_, Some(Result.Error(ex)), _ ->
                        textBasedError (ex.Message+"\n"+ex.ToString())
                    |_ ->
                        textBasedError ("bug: unreachable code")
                    |> function
                    |Result.Ok(httpStatus, contentType, bytesToSend, None) ->
                        ctx.Response.StatusCode <- httpStatus
                        ctx.Response.ContentType <- contentType
                        bytesToSend
                    |Result.Ok(httpStatus, contentType, bytesToSend, Some fileName) ->
                        ctx.Response.StatusCode <- httpStatus
                        ctx.Response.ContentType <- contentType

                        //filename* is for new browsers
                        let cntDisp = 
                            sprintf "attachment; filename=\"%s\"; filename*=UTF-8''%s" 
                                (Uri.EscapeDataString(fileName)) 
                                (fileName.Replace(' ', '_')) //Firefox doesn't like spaces in filenames...
                        ctx.Response.Headers.Add("Content-Disposition",  cntDisp |> StringValues.op_Implicit)
                        bytesToSend
                    |Result.Error(httpStatus, contentType, errorMessage) ->
                        ctx.Response.StatusCode <- httpStatus
                        ctx.Response.ContentType <- contentType
                        errorMessage
                
                do! ctx.Response.Body.WriteAsync(bytesToSend, 0, bytesToSend.Length) |> Async.AwaitTask
                return ()
            }
            |> Async.StartAsTask
            :> Task

        services.Add(
            {HttpMethodAndUrl.Url = descr.Url; Verb = descr.Method}, 
            System.Func<_,_> impl)

    let registerServices (di:IDiRegisterOnlyContainer) =
        let serviceInterfaces = 
            assemblies 
            |> ContractToImpl.getTypesDecoratedWithAttributeWithoutSubclasses<HttpService>
            |> List.ofSeq

        let serviceToImpl = 
            ContractToImpl.getImplementations assemblies serviceInterfaces 
            |> List.ofSeq

        serviceToImpl
        |> Seq.iter(fun x -> di.RegisterAlias(x.Contract, x.Implementation, LifeStyle.Scoped))

        di.Register<ClientConnectionInfo>(LifeStyle.Scoped)

        // bind services to http machinery
        Services.registerServices
            serviceToImpl
            di
            registerPostRequest
            (ServerSentRegistrator.Register)
            (fun url sub -> servSntEvListeners.Add(url, sub) )

        //invoke all DI installers
        let installer = typeof<IDiInstaller>
        let installers = 
            assemblies
            |> Seq.map(fun asm ->
                asm.GetTypes() |> Seq.filter(fun typex -> typex.IsClass && installer.IsAssignableFrom(typex) ))
            |> Seq.concat
            |> List.ofSeq
        
        installers
        |> List.iter(fun (x:Type) -> 
            sprintf "start invoking installer %s" x.FullName |> log
            let installer = Activator.CreateInstance(x) :?> IDiInstaller
            installer.Install(di)
            sprintf "ended installer invokation %s" x.FullName |> log)
            
    let buildRootedPath rootPth (pth:string) =
        if System.IO.Path.IsPathRooted pth
        then pth
        else Path.Combine(rootPth, pth) |> Path.GetFullPath

    let isUnderWindows : bool = Philadelphia.ServerSideUtils.OperatingSystem.isWindows ()

    let rec buildPath (basePath:string option) (elems:string list)  =
        //sprintf "buildPath basePath=%A elems=%A" basePath elems |> log
        
        match basePath,elems with
        |Some basePath, [] -> basePath
        |None, x::others -> buildPath (Some x) others            
        |Some basePath, x::others when x.Contains("*") ->
            Directory.EnumerateDirectories(basePath, x)
            |> List.ofSeq
            |> function
            |[x] -> buildPath (System.IO.Path.Combine(basePath, x) |> Some ) others
            |[] -> failwithf "nothing matches indirect directory %s%c%s" basePath Path.DirectorySeparatorChar x
            |_ -> failwithf "ambiguous indirect directory %s%c%s" basePath Path.DirectorySeparatorChar x
        |Some basePath, x::others -> buildPath (System.IO.Path.Combine(basePath, x) |> Some) others        
        |_ -> failwith "bug: unsupported state"

    let getFullPathResolvingAsterisks (relativeToDir:string) (pth:string) =
        if not <| pth.Contains("*")
        then buildRootedPath relativeToDir pth
        else
            //paths should use native separator under nonwindows
            //under Windows may use both slash and backslash
            let dirSep = if isUnderWindows then [| Path.DirectorySeparatorChar; '/' |] else [| Path.DirectorySeparatorChar |]
            let elems = pth.Split(dirSep) |> List.ofArray
                
            let res =
                buildPath
                    (
                     match Path.IsPathRooted pth, isUnderWindows with
                     |false, _ -> Some relativeToDir
                     |true, false -> Some "/"
                     |true, _ -> None )
                    elems
            sprintf "resolved indirect path %s as %s" pth res |> log
            res

    let registerStaticFiles (resources:StaticResourceDef seq) =            
        resources
        |> Seq.collect(fun ress ->
            sprintf "registering static resources from %s" ress.DefinedBy |> log
            let buildRootedPath = buildRootedPath ress.RelativeToPath
            let urlToResource =
                ress.Items
                |> Seq.collect(fun res ->                    
                    match res.FilePattern with
                    | null -> //single file
                        logf "singlemode static resource registering %s as %s" res.FileSystemPath res.Url
                        let itm =
                            res.Url, 
                            StaticFileResource(
                                FileSystemPath = buildRootedPath res.FileSystemPath,
                                MimeType = res.MimeType)
                        [ itm ] |> Seq.ofList
                    | patt -> //many files
                        let re = Regex(patt)
                        logf "multimode static resource registering, dir %s, pattern %s" res.Dir patt 
                        res.Dir
                        |> getFullPathResolvingAsterisks ress.RelativeToPath
                        |> Directory.EnumerateFiles
                        |> Seq.filter re.IsMatch
                        |> Seq.map(fun filePth -> 
                            let url = System.String.Format(res.Url, Path.GetFileName filePth)
                            let statRes =
                                StaticFileResource(
                                    FileSystemPath = filePth,
                                    MimeType = res.MimeType)
                            logf "file %s <=> URL %s" filePth url 
                            url, statRes)                          
                          )
            urlToResource)
        |> Map.ofSeq
        |> statics.AddRange
        
    let processResponse (ctx:HttpContext) : Task =
        let di = 
            ServiceProviderAdapterAsDiContainer(ctx.RequestServices) 
            :> IDiResolveReleaseOnlyContainer
             
        let maybeGetQueryItem x =
            if ctx.Request.Query.ContainsKey(x)
            then 
                let vals = ctx.Request.Query.[x] 
                if vals.Count > 0 
                then vals.Item(0) |> Some
                else None
            else None

        do
            let cci = di.Resolve<_>()
            initializeConnectionInfo cci ctx
            populateCsrfMaybe cci (fun () ->
                let name = Magics.CsrfTokenFieldName
                match ctx.Request.Headers.MaybeGetValue name, maybeGetQueryItem name with
                |Some x, _ when x.Count > 0 -> 
                    log "found CSRF token in request headers"
                    x.Item(0) |> Some
                |_, Some x -> 
                    log "found CSRF token in query string"
                    Some x
                |_ -> None)

            //optional dependency
            match di.TryResolve<IClientConnectionInfoConnectionIdProvider>() with
            | struct (true, provider) -> provider.Provide()
            | struct(false, _) -> System.Guid.NewGuid().ToString()
            |> cci.InitializeConnectionId

        let lifetimeFilter:ILifetimeFilter = di.Resolve()

        let pth = ctx.Request.Path.Value   
        
        let serviceImpl = 
            let pathItems = pth.Split("/")
            if pathItems.Length < 3 
            then None
            else
                {
                    HttpMethodAndUrl.Verb = ctx |> HttpMethodVerb.fromHttpContext 
                    Url = sprintf "/%s/%s" pathItems.[1] pathItems.[2]
                } |> Some
            |> Option.bind(services.MaybeGetValue)

        match serviceImpl, statics.MaybeGetValue pth, servSntEvListeners.MaybeGetValue pth with
        |None, Some statFile,  _ -> //static file
            async {
                let! connAct =
                    lifetimeFilter.OnConnectionBeforeHandler(di, pth, null, null, Array.empty, ResourceType.StaticResource)
                    |> Async.AwaitTask

                let sender =
                    match connAct.MaybeFiltered with
                    |Some filtered -> 
                        ctx.Response.StatusCode <- filtered.HttpStatus
                        ctx.Response.ContentType <- filtered.ContentType                        
                        ctx.Response.Body.WriteAsync(filtered.Content, 0, filtered.Content.Length)
                    |None ->
                        ctx.Response.ContentType <- statFile.MimeType        
                        statFile.FileSystemPath |> ctx.Response.SendFileAsync

                return! sender |> Async.AwaitTask
            }
            |> Async.StartAsTask
            :> Task
            
        |Some serviceImpl, _, _ -> //regular POST service            
            serviceImpl.Invoke ctx
        |_, _, Some mbox -> //server sent events listener
            let clientCtx = ctx.Request.Query.Item("i").Item(0)
            let connIdAsStreamId = (di.Resolve<ClientConnectionInfo>()).ConnectionId
            
            ctx.Response.Headers.Add("Content-Type", StringValues.op_Implicit "text/event-stream")
            ctx.Response.Headers.Add("Cache-Control", StringValues.op_Implicit "no-cache")
            let cancTkn = ctx.RequestAborted

            async {
            
                let! filterReply =
                    mbox.Subscribe di ctx.Response.Body clientCtx
                       
                return!
                    match filterReply with
                    |Some (reason:FilterReply) ->                    
                        ctx.Response.StatusCode <- reason.HttpStatus
                        ctx.Response.ContentType <- reason.ContentType
                        ctx.Response.Body.WriteAsync(reason.Content, 0, reason.Content.Length)
                        |> Async.AwaitTask
                    |None ->
                        log "client is accepted (filter is not null)"
                        let sleepingClient =
                            async {
                                //dual purpose: announce sseStreamId and send anything so that 'onopen' will be invoked in the browser
                                let msg = 
                                    sprintf 
                                        "event: %s\ndata: %s\n\n" 
                                            Philadelphia.Common.Model.Magics.SseStreamIdEventName
                                            connIdAsStreamId
                                let ack = Encoding.UTF8.GetBytes(msg)
                                do! ctx.Response.Body.WriteAsync(ack, 0, ack.Length) |> Async.AwaitTask
                                do! ctx.Response.Body.FlushAsync() |> Async.AwaitTask

                                log "starting sleeping in client async"
                                let! timeoutOrDisconnect =
                                    Task.Delay(serverSideEventSubscribentMaxLifeSeconds*1000, cancTkn) 
                                    |> Async.AwaitTask
                                    |> Async.Catch

                                match timeoutOrDisconnect with
                                |Choice1Of2(_) -> 
                                    log "finished sleeping in client async due to timeout"
                                |Choice2Of2(_) -> 
                                    log "finished sleeping in client async due to client disconnect"

                                mbox.Unsubscribe di ctx.Response.Body //timeout
                            }
                        Async.StartAsTask(sleepingClient)
                        |> Async.AwaitTask
            }
            |> Async.StartAsTask
            :> Task
        |_ ->
            async {
                let! filterReply =
                    lifetimeFilter.OnConnectionBeforeHandler(
                        di, pth, null, null, Array.empty, ResourceType.Unsupported)
                    |> Async.AwaitTask

                sprintf "unsupported path %s" pth |> log
                ctx.Response.StatusCode <- 400
                
                let unsupportedUrlResponse = 
                    match sett.CustomUnsupportedUrlResponseTemplate |> Option.ofObj with
                    |Some x -> 
                        try
                            String.Format(x, pth)
                        with
                        |ex->
                            sprintf "could not format unsupportedUrlResponse due to %O" ex |> log 
                            "unsupported path " + pth
                            
                    |None -> "unsupported path " + pth
                    |> Encoding.UTF8.GetBytes

                do!
                    ctx.Response.Body.WriteAsync(unsupportedUrlResponse, 0, unsupportedUrlResponse.Length)
                    |> Async.AwaitTask
                return ()
            }
            |> Async.StartAsTask
            :> Task
            
    member __.ConfigureServices
        (services: IServiceCollection, 
         [<Optional;DefaultParameterValue(null)>]?lifetimeFilter:ILifetimeFilter) =
        let di = ServiceCollectionAdapterAsDiContainer(services) :> IDiRegisterOnlyContainer
             
        do 
            if sett.ResponseCompression <> Compression.Disabled
            then
                //make sure you use mitigations against https://en.wikipedia.org/wiki/BREACH
                services.AddResponseCompression(fun x -> 
                    x.EnableForHttps <- sett.ResponseCompression = Compression.HttpAndHttps) |> ignore

        let lifetimeFilter = 
            lifetimeFilter 
            |> Option.defaultValue(NullLifetimeFilter.Instance)
       
        di.RegisterFactoryMethod(
            (fun _ -> lifetimeFilter), 
            LifeStyle.Singleton)

        //static files
        let staticResources = 
            StaticResources.getStaticResourceSources
                (match sett.CustomStaticResourceDirs |> Option.ofObj with
                |Some customStaticResourceDirs -> customStaticResourceDirs
                |_ -> [defaultStaticResourcesDir] |> Seq.ofList)
                (sett.CustomStaticResourceFileName |> Option.ofObj |> Option.defaultValue  "static_resources.json")

        registerStaticFiles staticResources

        //regular POST services
        registerServices di
        additionalConfigureServices.Invoke services
    
    member __.Configure(app: IApplicationBuilder, env: IHostingEnvironment) =
        let di = 
            new ServiceProviderAdapterAsDiContainer(app.ApplicationServices) 
            :> IDiResolveReleaseOnlyContainer
            
        do
            if sett.ResponseCompression <> Compression.Disabled
            then 
                app.UseResponseCompression() |> ignore
                log "response compression: enabled"
        
        let lifeTime = app.ApplicationServices.GetRequiredService<IApplicationLifetime>()        
        lifeTime.ApplicationStarted.Register(fun () -> 
            use di = di.CreateScope ()
            let lifetimeFilter:ILifetimeFilter = di.Resolve()
            
            lifetimeFilter.OnServerStarted di
            |> Async.AwaitTask
            |> Async.RunSynchronously )
        |> ignore
        
        app.Run(RequestDelegate processResponse) |> ignore
    