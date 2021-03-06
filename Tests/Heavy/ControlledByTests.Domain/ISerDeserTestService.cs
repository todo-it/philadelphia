﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Philadelphia.Common;

namespace ControlledByTests.Domain {
    [HttpService]
    public interface ISerDeserService {
        Task<int> ProcessInt(int v);
        Task<DateTime> ProcessDateTime(DateTime v, bool isUtc);
        Task<string> ProcessString(string v);
        Task<long> ProcessLong(long v);
        Task<decimal> ProcessDecimal(decimal v);
    }
}
