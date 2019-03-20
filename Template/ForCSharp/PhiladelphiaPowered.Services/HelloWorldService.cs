using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Philadelphia.Common;
using Philadelphia.Server.Common;
using PhiladelphiaPowered.Domain;

namespace PhiladelphiaPowered.Services {
    public class HelloWorldService : IHelloWorldService {        
        private readonly ClientConnectionInfo _client;

        public HelloWorldService(ClientConnectionInfo client) {
            _client = client;
        }
        
        public Task<string> SayHello(string toWhom) {
            return Task.FromResult($"Hello {toWhom} connecting from IP {_client.ClientIpAddress}");
        }        
    }
}
