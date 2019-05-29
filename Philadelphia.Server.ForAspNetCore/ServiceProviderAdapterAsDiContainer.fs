namespace Philadelphia.Server.ForAspNetCore

open Microsoft.Extensions.DependencyInjection
open Philadelphia.Common
open System

type ScopedServiceProviderAdapterAsDiContainer(adapted:IServiceScope) =
    interface System.IDisposable with
        member __.Dispose () = adapted.Dispose()

    //read https://andrewlock.net/the-difference-between-getservice-and-getrquiredservice-in-asp-net-core/
    interface IDiResolveReleaseOnlyContainer with
        member __.ResolveAll t = adapted.ServiceProvider.GetServices(t)        
        member __.TryResolve(t) =
            let res = adapted.ServiceProvider.GetService(t)
            struct (res <> null, res)
        member __.Resolve t = adapted.ServiceProvider.GetRequiredService(t)
        member __.CreateScope () = failwith "nested scopes are not supported"
        member __.Release _ = () //seems to be not needed https://stackoverflow.com/questions/46799492/how-to-release-a-service-acquired-via-iserviceprovider


type ServiceProviderAdapterAsDiContainer(adapted:IServiceProvider) =
    interface System.IDisposable with
        member __.Dispose () = () //nothing to dispose really

    //see https://andrewlock.net/the-difference-between-getservice-and-getrquiredservice-in-asp-net-core/
    interface IDiResolveReleaseOnlyContainer with
        member __.ResolveAll t = adapted.GetServices t
        member __.TryResolve(t) =
            let res = adapted.GetService(t)
            struct (res <> null, res)
        member __.Resolve t = adapted.GetRequiredService t
        member __.CreateScope () =  
            new ScopedServiceProviderAdapterAsDiContainer(adapted.CreateScope())
            :> IDiResolveReleaseOnlyContainer
        member __.Release _ = () //seems to be not needed https://stackoverflow.com/questions/46799492/how-to-release-a-service-acquired-via-iserviceprovider
