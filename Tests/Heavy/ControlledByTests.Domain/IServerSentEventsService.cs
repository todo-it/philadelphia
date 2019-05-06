using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Philadelphia.Common;

namespace ControlledByTests.Domain {
    [HttpService]
    public interface IServerSentEventsService {
        Func<SomeNotif,bool> RegisterListener(SomeNotifFilter inp);
        Task<Unit> Publish(SomeNotif inp);
    }
}
