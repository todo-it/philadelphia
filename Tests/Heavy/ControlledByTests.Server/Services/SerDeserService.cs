using System;
using System.Threading.Tasks;
using ControlledByTests.Domain;
using Philadelphia.Testing.DotnetCore;

namespace ControlledByTests.Server.Services {
    public class SerDeserService : ISerDeserService {
        private readonly IRegisterServiceInvocation _trace;

        public SerDeserService(IRegisterServiceInvocation trace) {
            _trace = trace;
        }

        public Task<int> ProcessInt(int v) {
            _trace.RegisterServiceInvocation(typeof(ISerDeserService), nameof(ProcessInt), v);

            //adds to make sure that server's value was actually seen by client
            return Task.FromResult(v + MagicsForTests.Serialization.Int.ServerAddVal);
        }

        public Task<DateTime> ProcessDateTime(DateTime v, bool isUtc) {
            _trace.RegisterServiceInvocation(typeof(ISerDeserService), nameof(ProcessDateTime), v, isUtc);

            //adds to make sure that server's value was actually seen by client
            return Task.FromResult(v.AddDays(MagicsForTests.Serialization.DateTime.ServerAddDays));
        }

        public Task<string> ProcessString(string v) {
            _trace.RegisterServiceInvocation(typeof(ISerDeserService), nameof(ProcessString), v);

            //adds to make sure that server's value was actually seen by client
            return Task.FromResult(v + MagicsForTests.Serialization.String.ServerAddSuffix);
        }

        public Task<long> ProcessLong(long v) {
            _trace.RegisterServiceInvocation(typeof(ISerDeserService), nameof(ProcessLong), v);

            //adds to make sure that server's value was actually seen by client
            return Task.FromResult(v + MagicsForTests.Serialization.Long.ServerAdd);
        }
    }
}
