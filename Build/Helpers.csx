using System.Runtime.CompilerServices;
using System.Linq;
using System;
using System.Text.RegularExpressions;
using System.IO;

// https://github.com/filipw/dotnet-script#script-location
public static string GetScriptFolder([CallerFilePath] string path = null) => Path.GetDirectoryName(path);

public static string GetPathToMsbuild() {
    if (Environment.OSVersion.Platform != PlatformID.Win32NT) {
        return "msbuild";
    }

    var studios = BlackFox.VsWhere.VsInstances.GetAll();
    var pth1 = 
       (from studio in studios
        let pthx = Path.Combine(studio.InstallationPath, "MSBuild\\15.0\\Bin\\MSBuild.exe")		
        where File.Exists(pthx)
        select pthx)
        .FirstOrDefault();
		
    var pth2 = 
       (from studio in studios
		let pthx = Path.Combine(studio.InstallationPath, "MSBuild\\Current\\Bin\\MSBuild.exe")
        where File.Exists(pthx)
        select pthx)
        .FirstOrDefault();
		
    if (pth1 == null && pth2 == null) {
        var stds = String.Join("", studios.Select(x => "\n   " + x.InstallationPath));
        throw new Exception($"msbuild not detected (using visual studio detection). Visual studio paths found:[{stds} ]");
    }

    return pth1 != null ? pth1 : pth2;
}

public class Executor {
    private string workDir;
    public static bool OutputOnlyOnError {get; set; } = false;

    public static Executor WithinDir(string workDir) {
        return new Executor() {workDir = workDir};
    }

    public static void LoggedAction(Action x, string msg) {
        try {
            if (!OutputOnlyOnError) {
                Console.WriteLine(msg);
            }
            x();
        } catch(Exception) {
            if (OutputOnlyOnError) {
                Console.WriteLine(msg);
            }
            throw;
        }
    }
    
    /// Will return true if dir existed, false if didnt
    public static bool AssureDirExists(string dir) {
        Console.WriteLine($"AssureDirExists {dir}");

        if(Directory.Exists(dir)){
            Console.WriteLine($"'{dir}' already exists - not creating");
            return true;
        } else {
            Console.WriteLine($"'{dir}' does not exist - will create");
            Directory.CreateDirectory(dir);
            Console.WriteLine($"'{dir}' created");
            return false;
        }
    }

    public static void AssureEmptyDir(string dir, bool assureExists = true) {
        Console.WriteLine($"AssureEmptyDir {dir}");
        var shouldClean = 
            !assureExists
            ||
            AssureDirExists(dir);
            
        if(shouldClean) {
            Directory.EnumerateDirectories(dir).Select(x => {            
                LoggedAction(() => Directory.Delete(x, true), $"removing dir {x}");
                return false;
            }).ToList();

            Directory.EnumerateFiles(dir).Select(x => {            
                LoggedAction(() => File.Delete(x), $"removing file {x}");
                return false;
            }).ToList();;
        }
    }
    public static void AssureNoMatchingFiles(string dir, string mask) {
        Console.WriteLine($"AssureNoMatchingFiles {dir} {mask}");
        
        Directory.EnumerateFiles(dir, mask).Select(x => {            
            LoggedAction(() => File.Delete(x), $"removing file {x}");
            return false;
        }).ToList();
    }
    public void MoveFileBetweenDirs(string fileName, string fromDir, string toDir) {
        Console.WriteLine($"move file={fileName} fromDir={fromDir} toDir={toDir}");
        File.Move(Path.Combine(fromDir, fileName), Path.Combine(toDir, fileName));
    }

    public String Exe(string exeFile, string args) {
        Console.WriteLine($"starting {exeFile} {args}");
        var p = new System.Diagnostics.Process();
        var outp = new StringBuilder();
        p.StartInfo = new ProcessStartInfo {
            FileName = exeFile,
            Arguments = args,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            WorkingDirectory = workDir
        };
        p.Start();
        while (!p.StandardOutput.EndOfStream) {
            outp.AppendLine(p.StandardOutput.ReadLine());            
        }
        p.WaitForExit();
        if (!OutputOnlyOnError || p.ExitCode != 0) {
            Console.WriteLine(outp.ToString());
        }
        Console.WriteLine($"ended {exeFile} {args} with exit code {p.ExitCode}");
        if (p.ExitCode != 0) {
            throw new Exception("wrong exitcode");
        }
	return outp.ToString();
    }
}

public static string GetVersionNoFromGitTag() {
    try {
        return new Executor().Exe("git", "describe --tags --abbrev=0").Trim();
    } catch (Exception ex) {
        Console.WriteLine($"could not find tag name using git command. falling back to library because of {ex}");
    }

    var repo = new LibGit2Sharp.Repository("..");
    var branchName = repo.Head.FriendlyName;
    var shaToTagName = repo.Tags.ToDictionary(x => x.Reference.TargetIdentifier, x => x.FriendlyName);
    
    var maybeTag = repo.Head.Commits.FirstOrDefault(x => shaToTagName.ContainsKey(x.Sha));

    if (maybeTag == null) {
        throw new Exception("version detection failed - current branch has no tagged commit");
    }

    return shaToTagName[maybeTag.Sha];
}

class Replacement {
    public Regex FileName {get; }
    public Regex[] PhrasesWithOneGrp {get; }
    
    public Replacement(string fileNameRe, params string[] phraseRe) {
        FileName = new Regex(fileNameRe);
        PhrasesWithOneGrp = phraseRe.Select(x => new Regex(x)).ToArray();
    }
}

class VersionUpdater {
    private static string GetMutated(Regex re, string within, string newVersion) {
        var at = 0;

        while (at < within.Length) {
            var matcher = re.Match(within, at);

            if (!matcher.Success) {
                break;
            }

            var verAt = matcher.Groups[1].Index;
            var verLen = matcher.Groups[1].Length;

            within = 
                within.Substring(0, verAt) + 
                newVersion +
                within.Substring(verAt + verLen);

            at = verAt + verLen;
        }

        return within;
    }

    private static void Mutate(string file, Replacement r, string newVer) {
        var o = File.ReadAllText(file);
        var n = o;
        foreach (var x in r.PhrasesWithOneGrp) {
            n = GetMutated(x, n, newVer);
        }
        if (o.Equals(n)) {
            Console.WriteLine($"not mutating {file} as change would be irrelevant");
            return;
        }
        Console.WriteLine($"updating version in {file}");
        File.WriteAllText(file, n);
    }

    private static void Impl(string baseDir, string newVersion, string[] dirsToIgnore, params Replacement[] replacements) {
        if (dirsToIgnore.Contains(Path.GetFileName(baseDir))) {
            return;
        }

        var files = Directory
            .GetFiles(baseDir)
            .Select(x => Tuple.Create(Path.GetFileName(x), x))
            .SelectMany(fnAndPath => 
                replacements
                    .Select(y => Tuple.Create(y.FileName.Match(fnAndPath.Item1).Success, fnAndPath.Item2, y))
                    .Where(y => y.Item1)
                    .Select(y => Tuple.Create(y.Item3, y.Item2)));

        foreach (var file in files) {
            Mutate(file.Item2, file.Item1, newVersion);
        }

        var dirs = Directory.GetDirectories(baseDir);
        
        foreach (var dir in dirs) {
            Impl(dir, newVersion, dirsToIgnore, replacements);
        }
    }

    public static void ChangeTo(string baseDir, string newVersion, string[] dirsToIgnore, params Replacement[] replacements) {
        Impl(baseDir, newVersion, dirsToIgnore, replacements);
    }
}
