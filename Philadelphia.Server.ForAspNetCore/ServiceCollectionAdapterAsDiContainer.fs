namespace Philadelphia.Server.ForAspNetCore

open Microsoft.Extensions.DependencyInjection
open Philadelphia.Common
open System
open System.Runtime.InteropServices

type LifeStyleContainer() =
    let mutable ls : LifeStyle option = None

    member __.get () = ls
    member __.set (v) = ls <- Some v

type ServiceCollectionAdapterAsDiContainer
    (adapted:IServiceCollection, 
    defaultLifeStyleCtn:LifeStyleContainer) =
    interface IDiRegisterOnlyContainer with
        member __.SetDefaultLifeStyle(style) = defaultLifeStyleCtn.set style

        member __.RegisterFactoryMethod(keyType:Type, factoryMethod:Func<_,_>, style) =
            let register = System.Func<_,_> (ServiceProviderAdapterAsDiContainer >> factoryMethod.Invoke)    
            
            style 
            |> Option.ofNullable
            |> function
            |None -> defaultLifeStyleCtn.get ()
            |Some x -> Some x
            |> function
            |Some LifeStyle.Scoped -> adapted.AddScoped(keyType, register)
            |Some LifeStyle.Singleton -> adapted.AddSingleton(keyType, register)
            |Some LifeStyle.Transient -> adapted.AddTransient(keyType, register)
            |_ -> failwith "unsupported scope"
            |> ignore

        member __.RegisterAlias(key, actualType, style) =            
            style 
            |> Option.ofNullable
            |> function
            |None -> defaultLifeStyleCtn.get ()
            |Some x -> Some x
            |> function
            |Some LifeStyle.Scoped -> adapted.AddScoped(key, actualType)
            |Some LifeStyle.Singleton -> adapted.AddSingleton(key, actualType)
            |Some LifeStyle.Transient -> adapted.AddTransient(key, actualType)
            |_ -> failwith "unsupported scope"
            |> ignore
