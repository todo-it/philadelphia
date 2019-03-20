using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Philadelphia.ServerSideUtils;
using Philadelphia.Common;
using Philadelphia.Server.Common;
using Philadelphia.Server.ForAspNetCore;

namespace CustomIView.Server {
    public class Startup {
        private readonly BaseStartup _baseStartup;

        public Startup() {
            var assemblies = new [] {
                typeof(Startup).Assembly,
                typeof(CustomIView.Domain.Dummy).Assembly,
                typeof(CustomIView.Services.Dummy).Assembly };

            var dllsLoc = Path.GetDirectoryName(typeof(Startup).Assembly.Location);
            Directory.SetCurrentDirectory(dllsLoc); //to make configuration reading from disk working

            var staticResourcesDir = Path.Combine(dllsLoc, "../../..");
            
            Logger.ConfigureImplementation(new ConsoleWritelineLoggerImplementation());
            _baseStartup = new BaseStartup(
                _ => {}, 
                assemblies,
                ServerSettings.CreateDefault()
                    .With(x => x.CustomStaticResourceDirs = new []{ staticResourcesDir }));
        }

        public void ConfigureServices(IServiceCollection services) {
            _baseStartup.ConfigureServices(services);
        }
        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
            _baseStartup.Configure(app, env);
        }
    }

    public class Program {
        public static void Main(string[] args) {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) {
            Logger.ConfigureImplementation(new ConsoleWritelineLoggerImplementation()); 

            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls(Configuration.getConfigVarOrDefault("http://0.0.0.0:8090/", "SERVER_LISTEN_URL"));
        }
    }
}
