using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphia.Common {
    //TODO consider renaming it to DateTimePrecision. DateTimeFormat as such should be derived from locale string and mapped onto some other type
    public enum DateTimeFormat {
        DateOnly,
        Y,
        YM,
        YMDhm,
        YMDhms
    }
}
