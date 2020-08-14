using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Philadelphia.ServerSideUtils;
using Philadelphia.Common;
using Philadelphia.Server.ForAspNetCore;

namespace Philadelphia.Demo.Server {
    public class Program {
        public static void Main(string[] args) {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) {
            var n = DateTime.Now;
            var dllsLoc = Path.GetDirectoryName(typeof(Program).Assembly.Location);

            Directory.SetCurrentDirectory(dllsLoc); //to make configuration reading from disk working

            var logDest = Configuration.getConfigVarOrDefault("DISK", "LOG_DESTINATION");

            if ("DISK" == logDest) {
                var destLogDir = 
                    Configuration.getConfigVarOrDefault(Path.Combine(dllsLoc, "log"), "LOG_DIR");

                Directory.CreateDirectory(destLogDir);

                string LogFileNameProvider() => 
                    Path.Combine(destLogDir, $"{n:yyyy-MM-dd_HH-mm-ss}.txt");

                Logger.ConfigureImplementation(new ForwardingLogger(LogFileNameProvider));
            } else if ("CONSOLE" == logDest) {
                Logger.ConfigureImplementation(new ConsoleWritelineLoggerImplementation());
            } else {
                Console.WriteLine("unknown log destination");
                throw new Exception("unknown log destination");
            }

            return WebHost
                .CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls(Configuration.getConfigVarOrDefault("http://0.0.0.0:8090/", "SERVER_LISTEN_URL"));
        }
    }
}
