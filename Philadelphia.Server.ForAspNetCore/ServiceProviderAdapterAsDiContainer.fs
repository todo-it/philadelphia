namespace Philadelphia.Server.ForAspNetCore

open Microsoft.Extensions.DependencyInjection
open Philadelphia.Common
open System

type ScopedServiceProviderAdapterAsDiContainer(adapted:IServiceScope) =
    interface System.IDisposable with
        member __.Dispose () = adapted.Dispose()

    interface IDiResolveReleaseOnlyContainer with
        member __.ResolveAll t = adapted.ServiceProvider.GetServices(t)
        member __.Resolve t = adapted.ServiceProvider.GetService(t)
        member __.CreateScope () = failwith "nested scopes are not supported"
        member __.Release _ = () //seems to be not needed https://stackoverflow.com/questions/46799492/how-to-release-a-service-acquired-via-iserviceprovider


type ServiceProviderAdapterAsDiContainer(adapted:IServiceProvider) =
    interface System.IDisposable with
        member __.Dispose () = () //nothing to dispose really

    interface IDiResolveReleaseOnlyContainer with
        member __.ResolveAll t = adapted.GetServices t
        member __.Resolve t = adapted.GetService t
        member __.CreateScope () =  
            new ScopedServiceProviderAdapterAsDiContainer(adapted.CreateScope())
            :> IDiResolveReleaseOnlyContainer
        member __.Release _ = () //seems to be not needed https://stackoverflow.com/questions/46799492/how-to-release-a-service-acquired-via-iserviceprovider
