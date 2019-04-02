using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ControlledByTests.Api;
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

namespace ControlledByTests.Server {
    public class ForwardToServerControllerLoggerImplementation : ILoggerImplementation {
        private readonly ConsoleBasedControllerAsLifeTimeFilter _impl;

        public ForwardToServerControllerLoggerImplementation(ConsoleBasedControllerAsLifeTimeFilter impl) {
            _impl = impl;
        }

        private void LogImpl(string level, Type sender, string message, object[] args) {
            _impl.LogAsReply(
                DateTime.UtcNow.ToString("o") + 
                " " +
                this.FlattenSafe(level, sender, message, args));
        }

        public void Error(Type sender, string message, params object[] args) {
            LogImpl("ERROR", sender, message, args);
        }

        public void Info(Type sender, string message, params object[] args) {
            LogImpl("INFO", sender, message, args);
        }

        public void Debug(Type sender, string message, params object[] args) {
            LogImpl("DEBUG", sender, message, args);
        }
    }

    public class Startup {
        private readonly BaseStartup _baseStartup;
        private readonly ConsoleBasedControllerAsLifeTimeFilter _ltFilter;

        public Startup() {
            var assemblies = new [] {
                typeof(Startup).Assembly,
                typeof(ControlledByTests.Domain.Dummy).Assembly,
                typeof(ControlledByTests.Services.Dummy).Assembly };

            var dllsLoc = Path.GetDirectoryName(typeof(Startup).Assembly.Location);
            Directory.SetCurrentDirectory(dllsLoc); //to make configuration reading from disk working

            _ltFilter = new ConsoleBasedControllerAsLifeTimeFilter();
            var staticResourcesDir = Path.Combine(dllsLoc, "../../..");
            
            Logger.ConfigureImplementation(new ForwardToServerControllerLoggerImplementation(_ltFilter));
            _baseStartup = new BaseStartup(
                _ => {}, 
                assemblies,
                ServerSettings.CreateDefault()
                    .With(x => x.CustomStaticResourceDirs = new []{ staticResourcesDir }));
        }

        public void ConfigureServices(IServiceCollection services) {
            var di = new ServiceCollectionAdapterAsDiContainer(services);

            di.RegisterInstance<IRegisterServiceInvocation>(_ltFilter, LifeStyle.Singleton);

            _baseStartup.ConfigureServices(services, _ltFilter);
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
                .UseUrls(Configuration.getConfigVarOrDefault("http://0.0.0.0:8090/", "SERVER_LISTEN_URL"))
                .SuppressStatusMessages(true) //to disable messages such as "Hosting environment: <blah>" etc
                .ConfigureLogging(b => b.AddFilter(_ => false)); //to disable per request messages
        }
    }
}
