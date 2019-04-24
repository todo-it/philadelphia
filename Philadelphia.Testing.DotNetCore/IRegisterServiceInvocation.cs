using System;

namespace Philadelphia.Testing.DotNetCore {
    public interface IRegisterServiceInvocation {
        void RegisterServiceInvocation(Type iface, string methodName, params object[] parameters);
    }
}
