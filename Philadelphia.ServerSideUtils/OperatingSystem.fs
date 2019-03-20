module Philadelphia.ServerSideUtils.OperatingSystem

open System.Runtime.InteropServices

let isLinux () = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
let isWindows () = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
