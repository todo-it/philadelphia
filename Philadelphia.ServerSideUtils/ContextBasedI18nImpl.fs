module Philadelphia.ServerSideUtils.ContextBasedI18nImpl

open Philadelphia.Common

type ICurrentLanguageStorage =
    abstract member getLangCode : unit -> string option
    abstract member setLangCode : code:string -> unit 

let calculateDefaultCulture (translations:ITranslationProvider seq) =
    translations
    |> Seq.tryPick (fun x -> if x.IsDefault then Some x else None)
    |> function 
    |Some x -> x.CultureName
    |None -> 
        Logger.Debug(
            typeof<obj>, 
            sprintf "have %d translations but none of them is default - using English" (Seq.length translations))
        "en-US"

let buildLangToMessageToTranslation translations =
    translations
    |> Seq.map (fun (x:ITranslationProvider) ->
        let items = x.Items |> Seq.map (fun x -> x.M, x.T) |> Map.ofSeq
        x.CultureName, items )
    |> Map.ofSeq
            
let getCurrentLanguage defaultCulture (storage:ICurrentLanguageStorage) = 
    storage.getLangCode () |> Option.defaultValue defaultCulture
                
let translate langToMessageToTranslation msg lang = 
    match Map.tryFind lang langToMessageToTranslation with
    |Some transl -> 
        match Map.tryFind msg transl with
        |Some x -> x
        |_ -> msg
    |_ -> msg
