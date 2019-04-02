using System;

namespace ControlledByTests.Api {
    public interface IRegisterServiceInvocation {
        void RegisterServiceInvocation(Type iface, string methodName, params object[] parameters);
    }
}
