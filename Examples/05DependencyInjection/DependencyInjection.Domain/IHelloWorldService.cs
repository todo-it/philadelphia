using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Philadelphia.Common;

namespace DependencyInjection.Domain {
    [HttpService]
    public interface IHelloWorldService {
        Task<string> SayHello(string toWhom);        
    }
}
