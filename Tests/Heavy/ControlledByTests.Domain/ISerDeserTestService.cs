using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Philadelphia.Common;

namespace ControlledByTests.Domain {
    [HttpService]
    public interface ISerDeserService {
        Task<int> ProcessInt(int v);
        Task<DateTime> ProcessDateTime(DateTime v, bool isUtc);
        Task<string> ProcessString(string str);
    }
}
