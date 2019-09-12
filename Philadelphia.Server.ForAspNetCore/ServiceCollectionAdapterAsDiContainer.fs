namespace Philadelphia.Server.ForAspNetCore

open Microsoft.Extensions.DependencyInjection
open Philadelphia.Common
open System

type ServiceCollectionAdapterAsDiContainer(adapted:IServiceCollection) =
    interface IDiRegisterOnlyContainer with
        member __.RegisterFactoryMethod(keyType:Type, factoryMethod:Func<_,_>, style) =
            let register = System.Func<_,_> (ServiceProviderAdapterAsDiContainer >> factoryMethod.Invoke)

            match style with 
            |LifeStyle.Scoped -> adapted.AddScoped(keyType, register)
            |LifeStyle.Singleton -> adapted.AddSingleton(keyType, register)
            |LifeStyle.Transient -> adapted.AddTransient(keyType, register)
            |_ -> failwith "unsupported scope"
            |> ignore

        member __.RegisterAlias(key, actualType, style) =
            match style with
            |LifeStyle.Scoped -> adapted.AddScoped(key, actualType)
            |LifeStyle.Singleton -> adapted.AddSingleton(key, actualType)
            |LifeStyle.Transient -> adapted.AddTransient(key, actualType)
            |_ -> failwith "unsupported scope"
            |> ignore
