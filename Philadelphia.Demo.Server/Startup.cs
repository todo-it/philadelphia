using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FSharp.Core;
using Philadelphia.Common;
using Philadelphia.Demo.SharedModel;
using Philadelphia.Server.ForAspNetCore;
using Philadelphia.Server.Common;

namespace Philadelphia.Demo.Server {
    /// <summary>
    /// favorite logger may be used here so that logging library is not part of Philadelphia.ServerSide
    /// Serilog? NLog? Microsoft.Extensions.Logging?
    /// </summary>
    class LifeTimeFilter : ILifetimeFilter {
        public Task OnServerStarted(IDiResolveReleaseOnlyContainer di) {
            Logger.Debug(typeof(LifeTimeFilter), "OnServerStarted()");
            return Task.CompletedTask;
        }

        public Task<ConnectionAction> OnConnectionBeforeHandler(
                IDiResolveReleaseOnlyContainer di, string url, object serviceInstance, 
                MethodInfo method, object[] prms, ResourceType res) {

            var ci = di.Resolve<ClientConnectionInfo>();
            var guid = Guid.NewGuid().ToString();
            Logger.Debug(typeof(LifeTimeFilter), $"OnConnectionBeforeHandler() ip={ci.ClientIpAddress} guid={guid} url={url} resourceType={res.ToString()} serviceImpl={serviceInstance} method={method} prms={prms}");
            return Task.FromResult(ConnectionAction.CreateNonFiltered(guid));
        }

        public Task OnConnectionAfterHandler(object connCtx, IDiResolveReleaseOnlyContainer obj0, Exception exOrNull) {
            Logger.Debug(typeof(LifeTimeFilter), $"OnConnectionAfterServiceHandler() guid={connCtx} success?={exOrNull == null} errorDetails={exOrNull}");
            return Task.CompletedTask;
        }
    }
    
    public class Startup {
        private readonly BaseStartup _baseStartup;
        private readonly LifeStyleContainer _lsc = new LifeStyleContainer();
        
        public Startup() {
            _lsc.set(LifeStyle.Transient); //least surprising
            var dllsLoc = System.IO.Path.GetDirectoryName(typeof(Startup).Assembly.Location);
            var assemblies = new [] {
                typeof(Startup).Assembly,
                typeof(SharedModel.Dummy).Assembly,
                typeof(ServicesImpl.Dummy).Assembly };

            _baseStartup = new BaseStartup(
                _lsc,
                _ => {}, 
                assemblies,
                ServerSettings.CreateDefault());
        }

        public void ConfigureServices(IServiceCollection services) {
            _baseStartup.ConfigureServices(services, new LifeTimeFilter());
        }
        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
            _baseStartup.Configure(app, env);
        }
    }
}
