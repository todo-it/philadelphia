using System;
using System.Threading.Tasks;
using ControlledByTests.Api;
using ControlledByTests.Domain;

namespace ControlledByTests.Services {
    public class SerDeserService : ISerDeserService {
        private readonly IRegisterServiceInvocation _trace;

        public SerDeserService(IRegisterServiceInvocation trace) {
            _trace = trace;
        }

        public Task<int> ProcessInt(int v) {
            _trace.RegisterServiceInvocation(typeof(ISerDeserService), nameof(ProcessInt), v);

            //adds to make sure that server's value was actually seen by client
            return Task.FromResult(v + MagicsForTests.SerDeser_Int_ServerAddVal);
        }

        public Task<DateTime> ProcessDateTime(DateTime v, bool isUtc) {
            _trace.RegisterServiceInvocation(typeof(ISerDeserService), nameof(ProcessDateTime), v, isUtc);

            //adds to make sure that server's value was actually seen by client
            return Task.FromResult(v.AddDays(MagicsForTests.SerDeser_DateTime_ServerAddDays));
        }

        public Task<string> ProcessString(string v) {
            _trace.RegisterServiceInvocation(typeof(ISerDeserService), nameof(ProcessString), v);

            //adds to make sure that server's value was actually seen by client
            return Task.FromResult(v + MagicsForTests.SerDeser_String_ServerAddSuffix);
        }
    }
}
