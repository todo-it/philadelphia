module Philadelphia.ServerSideUtils.CustomStorageI18nImpl

open Philadelphia.Common
open ContextBasedI18nImpl

type Implementation(storage:ICurrentLanguageStorage, translations:ITranslationProvider[]) =     
    let defaultCulture = calculateDefaultCulture translations
    let translations = buildLangToMessageToTranslation translations
    
    interface ICurrentCultureSwitchedListener with
        member __.OnSwitchedTo inp = storage.setLangCode inp

    interface I18nImpl with    
        member __.Translate msg = 
            translate translations msg (getCurrentLanguage defaultCulture storage)
            
        member __.TranslateForLang (msg,lang) = 
            translate translations msg lang   
