module Philadelphia.ServerSideUtils.AsyncLocalBasedI18nImpl

open Philadelphia.Common
open System.Text.RegularExpressions
open ContextBasedI18nImpl
open Philadelphia.ServerSideUtils.AsyncBasedStorage

///Language-switching in runtime TranslationImplementation
///warning, it is prone to gotcha https://stackoverflow.com/questions/37306203/how-to-make-asynclocal-flow-to-siblings
type Implementation(translations:ITranslationProvider[]) =
    let storage = AsyncBasedStorage() :> ICurrentLanguageStorage 
        
    let defaultCulture = calculateDefaultCulture translations
    let translations = buildLangToMessageToTranslation translations
    
    interface ICurrentCultureSwitchedListener with
        member __.OnSwitchedTo inp = storage.setLangCode inp

    interface I18nImpl with    
        member __.Translate msg = 
            translate translations msg (getCurrentLanguage defaultCulture storage)
            
        member __.TranslateForLang (msg,lang) = 
            translate translations msg lang

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
