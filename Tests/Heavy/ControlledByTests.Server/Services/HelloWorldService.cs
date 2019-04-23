using System.Threading.Tasks;
using ControlledByTests.Domain;
using Philadelphia.Common;
using Philadelphia.Testing.DotnetCore;

namespace ControlledByTests.Server.Services {
    public class HelloWorldService : IHelloWorldService {        
        private readonly ClientConnectionInfo _client;
        private readonly IRegisterServiceInvocation _trace;

        public HelloWorldService(ClientConnectionInfo client, IRegisterServiceInvocation trace) {
            _client = client;
            _trace = trace;
        }
        
        public Task<string> SayHello(string toWhom) {
            _trace.RegisterServiceInvocation(typeof(IHelloWorldService), nameof(SayHello), toWhom);

            return Task.FromResult($"Hello {toWhom}. How are you?");
        }        
    }
}
