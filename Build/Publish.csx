#! "netcoreapp3.1"

//uses: https://github.com/filipw/dotnet-script
//to install: dotnet tool install -g dotnet-script
//to use: dotnet script Build.csx
//to debug/develop in VS Code: 
//  see https://github.com/filipw/dotnet-script section 'Scaffolding' 
//  dotnet script init
//  ...that creates json files so that you can 'open folder' in VS Code

#r "nuget: BlackFox.VsWhere, 1.0.0" //for msbuild detection
#r "nuget: LibGit2Sharp, 0.26.0" //take version from git tag
#load "Helpers.csx"

using System.Runtime.CompilerServices;
using System.Linq;
using System.Text.RegularExpressions;

var version = GetVersionNoFromGitTag();
Console.WriteLine($"Version: {version}");

var slnDir = Path.GetDirectoryName(GetScriptFolder());
Console.WriteLine($"Root dir: {slnDir}");

string msbuild = GetPathToMsbuild();
var nuget = Environment.OSVersion.Platform != PlatformID.Win32NT ? "nuget" : Path.Combine(slnDir, @"3rdPartyLibraries\nuget\nuget.exe");
var outputDir = Path.Combine(slnDir, "_output");

Executor.OutputOnlyOnError = true; //quiet unless error

Executor ex;

var toPublish = System.IO.Directory.EnumerateFiles(outputDir, "*.nupkg");

{
    ex = Executor.WithinDir(outputDir);

    foreach (var f in toPublish) {
        var packageName = Path.GetFileName(f);
        Console.WriteLine(packageName);

        //depends on: nuget setApiKey actual_API_key_from_nuget.org
        ex.Exe(nuget, $"push {packageName} -Source https://api.nuget.org/v3/index.json");
    }    
}
