module Philadelphia.ServerSideUtils.AsyncLocalBasedI18nImpl

open Philadelphia.Common
open System.Text.RegularExpressions

///Language-switching in runtime TranslationImplementation
type Implementation(translations:ITranslationProvider[]) =
    static let _store = System.Threading.AsyncLocal<string>()

    let defaultCulture =
        translations 
        |> Seq.tryPick (fun x -> if x.IsDefault then Some x else None)
        |> function 
        |Some x -> x.CultureName
        |None -> 
            Logger.Info(
                typeof<Implementation>, 
                sprintf "have %d translations but none of them is default - using English" translations.Length)
            "en-US"

    let langToMessageToTranslation = 
        // review: could use F# Map
        let res = System.Collections.Generic.Dictionary<_, _>()

        translations
        |> Seq.iter (fun x ->
            res.Add(
                x.CultureName, 
                x.Items |> Seq.map (fun x -> x.M, x.T) |> DictionaryExtensions.Create))
        res
            
    let getCurrentLanguage () = 
        let curLang = _store.Value

        if curLang = null then defaultCulture else curLang
        
    let setCurrentLanguage newLang = _store.Value <- newLang
                    
    let translate msg lang = 
        match langToMessageToTranslation.TryGetValue(lang) with
        |true,transl -> 
            match transl.TryGetValue(msg) with
            |true,x -> x
            |_ -> msg
        |_ -> msg

    interface ICurrentCultureSwitchedListener with
        member __.OnSwitchedTo inp = setCurrentLanguage inp

    interface I18nImpl with    
        member __.Translate msg = 
            translate msg (getCurrentLanguage ())
            
        member __.TranslateForLang (msg,lang) = 
            translate msg lang   

let loadTranslationForLang (json:Json.ICodec) fullPath lang isDefault =
    do 
        if not <| Regex("^[a-z]{2}-[A-Z]{2}$").Match(lang).Success 
        then failwithf "unsupported translation for lang %s" lang
        
    do if not <| System.IO.File.Exists(fullPath) then failwithf "cannot find translation file %s" fullPath
    
    {
        new ITranslationProvider with
            member __.IsDefault = isDefault
            member __.CultureName = lang
            member __.Items = json.Decode<TranslationRoot>(System.IO.File.ReadAllText fullPath).Items
    }
