using System;
using System.Diagnostics;
using System.Text;

namespace Philadelphia.Testing.DotNetCore.Selenium {
    public class ControlledServerBuilder {
        public static ControlledServerController Start(
                Action<string> logger, ICodec codec, TimeSpan defaultReadTimeout, 
                string startDirOrNull, string program, string args) {

            var result = new StringBuilder();
            var proc = new Process {
                StartInfo = {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    FileName = program,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    RedirectStandardError = true,
                },
                EnableRaisingEvents = true,
            };

            if (startDirOrNull != null) {
                logger($"custom start dir: {startDirOrNull}");
                proc.StartInfo.WorkingDirectory = startDirOrNull;
            }
            
            return new ControlledServerController(logger, codec, proc, defaultReadTimeout);
        }
    }
}
