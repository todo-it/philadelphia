using System;

namespace Philadelphia.Testing.DotnetCore {
    public interface IRegisterServiceInvocation {
        void RegisterServiceInvocation(Type iface, string methodName, params object[] parameters);
    }
}
