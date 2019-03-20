module Philadelphia.ServerSideUtils.Configuration

open System
    
let argumentsFileName = "_arguments.cfg"
let getArgumentOrNull (commandLine:string) argName = 
    let needed = "--"+argName+"=";
    let at = commandLine.IndexOf(needed, StringComparison.InvariantCulture)
    if at < 0 
    then null
    else
        let arg = commandLine.Substring(at+needed.Length)

        let isQuoted = arg.Length > 0 &&  arg.[0] = '\"'

        let arg = if isQuoted then arg.Substring(1) else arg
        
        let till = arg.IndexOf((if isQuoted then "\"" else " "), StringComparison.InvariantCulture)
        
        if till > 0 then arg.Substring(0, till) else arg
  
let getCommandLineArgumentOrNull argName = getArgumentOrNull System.Environment.CommandLine argName

///command line OR environment OR "_arguments.cfg"
let getConfigVar name = 
    let cmdLine = getCommandLineArgumentOrNull name
    let env = System.Environment.GetEnvironmentVariable name

    if cmdLine <> null 
    then cmdLine |> Some
    else if env <> null 
    then env |> Some
    else
        if System.IO.File.Exists(argumentsFileName) 
        then
            let fakeCmdLine = 
                let x = System.IO.File.ReadAllText(argumentsFileName)
                
                let i = x.IndexOf('\r') 
                let x = if i>=0 then x.Substring(0, i) else x

                let i = x.IndexOf('\n') 
                if i>=0 then x.Substring(0, i) else x

            let argFromFile = getArgumentOrNull fakeCmdLine name
            if argFromFile <> null then argFromFile |> Some else None
        else None

let getConfigVarOrDefault def name = 
    match getConfigVar name with
    |Some x -> x
    |_ -> def

let getConfigVarOrFail name = 
    match getConfigVar name with
    |Some x -> x
    |_ -> failwithf "getConfigVarOrFail could not find nonnull value for field: %s" name

let stringToBool (inp:string) =
    match inp.ToLower() with
    |"1"|"true"|"yes"|"enable"|"enabled"|"on"|"y" -> true
    |"0"|"false"|"no"|"disable"|"disabled"|"off"|"n" -> false
    |_ -> failwith "cannot interpret value as true or false"
        
let getBoolConfigVarOrDefault def name = 
    match getConfigVar name with
    |Some x -> stringToBool x
    |_ -> def
        
let getBoolConfigVarOrFail name = 
    match getConfigVar name with
    |Some x -> stringToBool x
    |_ -> failwithf "getBoolConfigOrFail could not find nonnull value for field: %s" name

let getIntConfigVarOrDefault def name = 
    match getConfigVar name with
    |Some x -> System.Convert.ToInt32(x)
    |_ -> def
                
let getIntConfigVarOrFail name = 
    match getConfigVar name with
    |Some x -> System.Convert.ToInt32(x)
    |_ -> failwithf "getIntConfigOrFail could not find nonnull value for field: %s" name
