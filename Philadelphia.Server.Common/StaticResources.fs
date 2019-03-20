namespace Philadelphia.Server.Common

open System.Collections.Generic

type StaticResourceDef = {
    DefinedBy : string
    RelativeToPath : string
    Items : StaticResourceItem seq
}

module StaticResources =
    open System.IO

    let private getAbsDir = Path.GetFullPath >> Path.GetDirectoryName

    let deserializeFromFiles paths =
        let result = 
            paths
            |> Seq.map(fun pth -> pth, File.ReadAllText pth)
            |> Seq.map(fun (pth, json) ->
                let dir = getAbsDir pth
                let items =
                    Newtonsoft.Json.JsonConvert.DeserializeObject<List<StaticResourceItem>>(json)
                    
                {
                    StaticResourceDef.RelativeToPath = dir
                    DefinedBy = pth                    
                    Items = items
                })
             
        let broken =
            result
            |> Seq.collect(fun x ->
                x.Items
                |> Seq.where (fun y -> y.IsBroken)
                |> Seq.map(fun y -> x.DefinedBy, y))
            |> List.ofSeq
                            
        match broken with
        |[] -> result
        |broken ->
            let broken =
                broken
                |> List.map(fun (defFile,itm) -> sprintf "staticResourceFile=%s wrongItem=%A" defFile itm)
                
            "wrong static resources:" + System.String.Join("\n", broken)
            |> failwith 

    let getStaticResourceSources fromDirs fileSearchPattern =
        let paths = 
            fromDirs
            |> Seq.map(fun dir -> System.IO.Directory.EnumerateFiles(dir, fileSearchPattern))
            |> Seq.concat
            |> Seq.distinct
            |> Seq.map(fun pth -> System.IO.Path.GetFullPath(pth))
        
        deserializeFromFiles paths 
        