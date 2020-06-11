using System;
using System.Threading.Tasks;
using Philadelphia.Common;

namespace Philadelphia.Web {
    public class TypeSafePromise<T> : IPromise {
        private readonly Action<Action<T>, Action<Exception>> _impl;
            
        public TypeSafePromise(Action<Action<T>,Action<Exception>> impl) {
            _impl = impl;
            Logger.Debug(GetType(), "created()");
        }

        public void Then(Delegate fulfilledHandler, Delegate errorHandler = null, Delegate progressHandler = null) {
            Logger.Debug(GetType(), "Then() starting");
            _impl(
                succ => {
                    Logger.Debug(GetType(), "Then _impl invoking successHandler {0}", succ);
                    fulfilledHandler?.Call(this, succ);
                }, 
                err => {
                    Logger.Debug(GetType(), "Then _impl invoking errorHandler {0}", err);
                    errorHandler?.Call(this, err);
                });
        }
    }
}
