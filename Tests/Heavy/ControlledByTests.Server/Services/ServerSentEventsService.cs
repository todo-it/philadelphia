using System;
using System.Threading.Tasks;
using ControlledByTests.Domain;
using Philadelphia.Common;
using Philadelphia.Server.Common;
using Philadelphia.Testing.DotNetCore;

namespace ControlledByTests.Server.Services {
    public class ServerSentEventsService : IServerSentEventsService {
        private readonly Subscription<SomeNotif, SomeNotifFilter> _subs;
        private readonly IRegisterServiceInvocation _trace;

        public ServerSentEventsService(
                Subscription<SomeNotif,SomeNotifFilter> subs, IRegisterServiceInvocation trace) {

            _subs = subs;
            _trace = trace;
        }

        public Task<Unit> Publish(SomeNotif inp) {
            _trace.RegisterServiceInvocation(typeof(IServerSentEventsService), nameof(Publish), inp);

            _subs.SendMessage(inp);
            return Task.FromResult(Unit.Instance);
        }

        public Func<SomeNotif, bool> RegisterListener(SomeNotifFilter inp) {
            _trace.RegisterServiceInvocation(
                typeof(IServerSentEventsService), nameof(RegisterListener), inp);

            if (inp.DontAcceptMe) {
                return null;
            }

            //when true message is forwarded to particular client
            return x => {
                _trace.RegisterServiceInvocation(
                    typeof(IServerSentEventsService), nameof(RegisterListener), inp, x);

                if (x.Num > 0 && !inp.AcceptPositive) {
                    return false;
                }

                if (x.Num < 0 && !inp.AcceptNegative) {
                    return false;
                }

                if (x.Num % 2 == 0 && !inp.AcceptEven) {
                    return false;
                }

                if (x.Num % 2 == 1 && !inp.AcceptOdd) {
                    return false;
                }

                return true;
            };
        }
    }
}
