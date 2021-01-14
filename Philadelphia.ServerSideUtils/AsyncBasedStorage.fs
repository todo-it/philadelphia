module Philadelphia.ServerSideUtils.AsyncBasedStorage

open ContextBasedI18nImpl

type AsyncBasedStorage() =
    let _store = System.Threading.AsyncLocal<string>()
    
    interface ICurrentLanguageStorage with
        member __.getLangCode () = _store.Value |> Option.ofObj
        member __.setLangCode code = _store.Value <- code
