namespace Philadelphia.Server.ForAspNetCore

open Philadelphia.Common
open System.IO

type ForwardingLogger(filePathProvider:System.Func<string>) =
    [<VolatileField>]
    let mutable curPath = filePathProvider.Invoke()

    let mutable outp = StreamWriter(curPath)
    let lck = new obj()

    let changeWriterIfNeccessary () =
        let newPath = filePathProvider.Invoke()
        if newPath <> curPath 
        then
            lock lck (fun () ->
                curPath <- newPath
                outp.Close()
                outp <- StreamWriter(curPath)
            )

    interface Philadelphia.Common.ILoggerImplementation with            
        member self.Error(sender, message, args) = 
            let msg = LoggerImplementationExtensions.FlattenSafe(self, "ERROR", sender, message, args)
                
            changeWriterIfNeccessary()

            lock lck (fun () -> 
                msg |> outp.WriteLine
                outp.Flush())
                
        member self.Info(sender, message, args) =
            let msg = LoggerImplementationExtensions.FlattenSafe(self, "INFO", sender, message, args)
                                
            changeWriterIfNeccessary()

            lock lck (fun () -> 
                msg |> outp.WriteLine
                outp.Flush())
                
        member self.Debug(sender, message, args) =
            let msg = LoggerImplementationExtensions.FlattenSafe(self, "DEBUG", sender, message, args)
                                
            changeWriterIfNeccessary()

            lock lck (fun () -> 
                msg |> outp.WriteLine
                outp.Flush())

        