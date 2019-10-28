namespace Philadelphia.Server.ForAspNetCore

open Microsoft.Extensions.DependencyInjection
open Philadelphia.Common
open System

type ServiceCollectionAdapterAsDiContainer(adapted:IServiceCollection) =
    interface IDiRegisterOnlyContainer with
        member __.RegisterFactoryMethod(keyType:Type, factoryMethod:Func<_,_>, style) =
            let register = System.Func<_,_> (ServiceProviderAdapterAsDiContainer >> factoryMethod.Invoke)

            match style |> Option.ofNullable with 
            |Some LifeStyle.Scoped -> adapted.AddScoped(keyType, register)
            |Some LifeStyle.Singleton -> adapted.AddSingleton(keyType, register)
            |Some LifeStyle.Transient -> adapted.AddTransient(keyType, register)
            |_ -> failwith "unsupported scope"
            |> ignore

        member __.RegisterAlias(key, actualType, style) =
            match style |> Option.ofNullable with
            |Some LifeStyle.Scoped -> adapted.AddScoped(key, actualType)
            |Some LifeStyle.Singleton -> adapted.AddSingleton(key, actualType)
            |Some LifeStyle.Transient -> adapted.AddTransient(key, actualType)
            |_ -> failwith "unsupported scope"
            |> ignore
