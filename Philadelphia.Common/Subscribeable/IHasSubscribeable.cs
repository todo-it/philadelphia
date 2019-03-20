using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphia.Common {
    public interface IHasSubscribeable {
        ISubscribeable Subscribeable { get; }
    }
}
