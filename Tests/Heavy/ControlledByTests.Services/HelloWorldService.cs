using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ControlledByTests.Api;
using Philadelphia.Common;
using Philadelphia.Server.Common;
using ControlledByTests.Domain;

namespace ControlledByTests.Services {
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
