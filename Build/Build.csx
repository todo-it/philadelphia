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
Executor.AssureEmptyDir(outputDir);

Executor.OutputOnlyOnError = true; //quiet unless error

VersionUpdater.ChangeTo("..", version, new [] {".vs", ".git", "bin", "obj", "packages"},
    new Replacement(@"(?i)^assemblyinfo\.[cf]s$", 
        "AssemblyVersion\\(\\\"([^\\\"]*)\\\"\\)", 
        "AssemblyFileVersion\\(\\\"([^\\\"]*)\\\"\\)"),
    new Replacement(@"(?i)\.nuspec$",
        @"<version>([^<]*)</version>",
        "<dependency id=\"Philadelphia.[^\"]+\" version=\"([^\"]+)\"[^>]+>"),
    new Replacement(@"(?i)packages.config$",
        "<package id=\"Philadelphia.[^\"]+\" version=\"([^\"]+)\"[^>]+>"),
    new Replacement(@"(?i)\.[cf]sproj$",
        @"<Version>([^<]*)</Version>",
        "<PackageReference Include=\"Philadelphia.[^\"]+\" Version=\"([^\"]+)\"[^>]+>",
        "<Reference Include=\"Philadelphia.[^,]+, Version=([^,]+),",
        @"<HintPath>..\\packages\\Philadelphia.[^0-9]+([^\\]+)\\"),
// now contains '*' so not needed anymore        
//    new Replacement(@"(?i)static_resources.json$",
//        @"/Philadelphia.StaticResources.([^/]+)/content"),
    new Replacement(@"(?i).csx$",
        "\"nuget: Philadelphia.[^,]+,\\s+([^\"]+)\""));

Executor.AssureEmptyDir(outputDir);
Executor ex;

{
    var projDir = Path.Combine(slnDir, "Template");
    ex = Executor.WithinDir(projDir);
    
    ex.Exe(nuget, "pack Philadelphia.Template.nuspec");
    ex.MoveFileBetweenDirs($"Philadelphia.Template.{version}.nupkg", projDir, outputDir);
}

{
    ex = Executor.WithinDir(slnDir);
    ex.Exe(nuget, "restore Philadelphia.Toolkit.And.Demo.sln");
}

{
    var projDir = Path.Combine(slnDir, "Philadelphia.Common");
    ex = Executor.WithinDir(projDir);

    Executor.AssureEmptyDir(Path.Combine(projDir, "bin"));
	Executor.AssureEmptyDir(Path.Combine(projDir, "obj"));

    ex.Exe("dotnet", "pack Philadelphia.Common.csproj -c Release");
    ex.MoveFileBetweenDirs($"Philadelphia.Common.{version}.nupkg", 
        Path.Combine(projDir, "bin/Release"), outputDir);
}

{
    var projDir = Path.Combine(slnDir, "Philadelphia.Common.AsBridgeDotNet");
    ex = Executor.WithinDir(projDir);

    Executor.AssureEmptyDir(Path.Combine(projDir, "bin"));
	Executor.AssureEmptyDir(Path.Combine(projDir, "obj"));
    ex.Exe(msbuild, "Philadelphia.Common.AsBridgeDotNet.csproj /target:clean");
    ex.Exe(msbuild, "Philadelphia.Common.AsBridgeDotNet.csproj /p:Configuration=Release /target:build");
    ex.Exe(nuget, "pack Philadelphia.Common.AsBridgeDotNet.nuspec");
    ex.MoveFileBetweenDirs($"Philadelphia.Common.AsBridgeDotNet.{version}.nupkg", projDir, outputDir);    
}

{
    var projDir = Path.Combine(slnDir, "Philadelphia.CodeGen.ForClient");
    ex = Executor.WithinDir(projDir);
    Executor.AssureEmptyDir(Path.Combine(projDir, "bin"));
	Executor.AssureEmptyDir(Path.Combine(projDir, "obj"));

    ex.Exe("dotnet", "pack Philadelphia.CodeGen.ForClient.csproj -c Release");
    ex.MoveFileBetweenDirs($"Philadelphia.CodeGen.ForClient.{version}.nupkg", 
        Path.Combine(projDir, "bin/Release"), outputDir);
}

{
    var projDir = Path.Combine(slnDir, "Philadelphia.Web");
    ex = Executor.WithinDir(projDir);

    Executor.AssureEmptyDir(Path.Combine(projDir, "bin"));
	Executor.AssureEmptyDir(Path.Combine(projDir, "obj"));
	
    ex.Exe(msbuild, "Philadelphia.Web.csproj /target:clean");
    ex.Exe(msbuild, "Philadelphia.Web.csproj /p:Configuration=Release /target:build");
    ex.Exe(nuget, "pack Philadelphia.Web.nuspec");
    ex.MoveFileBetweenDirs($"Philadelphia.Web.{version}.nupkg", projDir, outputDir);
}

{
    var projDir = Path.Combine(slnDir, "Philadelphia.StaticResources");
    ex = Executor.WithinDir(projDir);

    ex.Exe(nuget, "pack Philadelphia.StaticResources.nuspec");
    ex.MoveFileBetweenDirs($"Philadelphia.StaticResources.{version}.nupkg", projDir, outputDir);
}

{
    var projDir = Path.Combine(slnDir, "Philadelphia.Server.Common");
    ex = Executor.WithinDir(projDir);
    Executor.AssureEmptyDir(Path.Combine(projDir, "bin"));
	Executor.AssureEmptyDir(Path.Combine(projDir, "obj"));

    ex.Exe("dotnet", "pack Philadelphia.Server.Common.fsproj -c Release");
    ex.MoveFileBetweenDirs($"Philadelphia.Server.Common.{version}.nupkg", 
        Path.Combine(projDir, "bin/Release"), outputDir);
}

{
    var projDir = Path.Combine(slnDir, "Philadelphia.ServerSideUtils");
    ex = Executor.WithinDir(projDir);
    Executor.AssureEmptyDir(Path.Combine(projDir, "bin"));
	Executor.AssureEmptyDir(Path.Combine(projDir, "obj"));

    ex.Exe("dotnet", "pack Philadelphia.ServerSideUtils.fsproj -c Release");
    ex.MoveFileBetweenDirs($"Philadelphia.ServerSideUtils.{version}.nupkg", 
        Path.Combine(projDir, "bin/Release"), outputDir);
}

{
    var projDir = Path.Combine(slnDir, "Philadelphia.Server.ForAspNetCore");
    ex = Executor.WithinDir(projDir);
    Executor.AssureEmptyDir(Path.Combine(projDir, "bin"));
	Executor.AssureEmptyDir(Path.Combine(projDir, "obj"));

    ex.Exe("dotnet", "pack Philadelphia.Server.ForAspNetCore.fsproj -c Release");
    ex.MoveFileBetweenDirs($"Philadelphia.Server.ForAspNetCore.{version}.nupkg", 
        Path.Combine(projDir, "bin/Release"), outputDir);
}

Console.WriteLine("\nOK")
