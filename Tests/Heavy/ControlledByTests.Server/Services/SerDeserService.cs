using System;
using System.Threading.Tasks;
using ControlledByTests.Domain;
using Philadelphia.Testing.DotNetCore;

namespace ControlledByTests.Server.Services {
    public class SerDeserService : ISerDeserService {
        private readonly IRegisterServiceInvocation _trace;

        public SerDeserService(IRegisterServiceInvocation trace) {
            _trace = trace;
        }

        public Task<int> ProcessInt(int v) {
            _trace.RegisterServiceInvocation(typeof(ISerDeserService), nameof(ProcessInt), v);

            //adds to make sure that server's value was actually seen by client
            return Task.FromResult(v + MagicsForTests.Serialization.Int.ServerAdd);
        }

        public Task<DateTime> ProcessDateTime(DateTime v, bool isUtc) {
            _trace.RegisterServiceInvocation(typeof(ISerDeserService), nameof(ProcessDateTime), v, isUtc);

            //adds to make sure that server's value was actually seen by client
            return Task.FromResult(MagicsForTests.Serialization.MidDate(v, MagicsForTests.Serialization.DateTime.ServerAdd));
        }

        public Task<string> ProcessString(string v) {
            _trace.RegisterServiceInvocation(typeof(ISerDeserService), nameof(ProcessString), v);

            //adds to make sure that server's value was actually seen by client
            return Task.FromResult(v + MagicsForTests.Serialization.String.ServerAdd);
        }

        public Task<long> ProcessLong(long v) {
            _trace.RegisterServiceInvocation(typeof(ISerDeserService), nameof(ProcessLong), v);

            //adds to make sure that server's value was actually seen by client
            return Task.FromResult(v + MagicsForTests.Serialization.Long.ServerAdd);
        }

        public Task<decimal> ProcessDecimal(decimal v) {
            _trace.RegisterServiceInvocation(typeof(ISerDeserService), nameof(ProcessDecimal), v);
            return Task.FromResult(v + MagicsForTests.Serialization.Decimal.ServerAdd);
        }
    }
}
