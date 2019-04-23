namespace Philadelphia.Server.Common

open System
open System.Reflection
open System.Linq
open System.Collections.Generic

module ContractToImpl =
    let getTypesDecoratedWithAttribute<'T when 'T:>Attribute > (assemblies:Assembly seq) includeSubclasses =
        assemblies
        |> Seq.map(fun x ->
            x.GetTypes().Where(fun y -> 
                y.IsInterface && 
                y.GetCustomAttributes(typeof<'T>, includeSubclasses).Any() ))
        |> Seq.concat

    let getTypesDecoratedWithAttributeWithoutSubclasses<'T when 'T:>Attribute > assemblies =
        getTypesDecoratedWithAttribute<'T> assemblies false

    let findServiceImplemention (assemblies:Assembly seq) (contractType:Type) =
        let result =
            assemblies
            |> Seq.collect(fun asm -> 
                asm.GetTypes() |> Seq.filter(fun implType -> contractType.IsAssignableFrom(implType) && implType.IsClass))
            |> Seq.truncate 2
            |> List.ofSeq
    
        match result with
        |[implType] -> Ok implType
        |[] -> Error (sprintf "no implementation found for type %O" contractType.FullName)
        |impls -> 
            let impls = 
                impls
                |> Seq.map(fun i -> sprintf "<- %s @ %s" i.AssemblyQualifiedName i.Assembly.CodeBase)
                |> String.concat "\n"
            Error (sprintf "more than one implementation found for type %O:\n%s" contractType.FullName impls)

    let findServiceImplementionOrFail assemblies x =
        match (findServiceImplemention assemblies x) with
        |Error error -> failwith error
        |Ok x -> x

    let getImplementations assemblies (input:IEnumerable<Type>) : IEnumerable<ContractToImplementation> =
        input |> Seq.map(fun x -> ContractToImplementation(x, findServiceImplementionOrFail assemblies x))