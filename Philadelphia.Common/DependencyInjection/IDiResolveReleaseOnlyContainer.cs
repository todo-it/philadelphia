using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Philadelphia.Common {
    //why IDisposable? it is modeled after aspnet core DI
    public interface IDiResolveReleaseOnlyContainer : IDisposable {
        IEnumerable<object> ResolveAll(Type t);
        object Resolve(Type t);

        /// <summary>returns true if success</summary>
        (bool success, object result) TryResolve(Type t);
        void Release(object t);

        // Question: why not put this into separate interface? it is currently only used by server side DI
        /// <summary>
        /// creates scope that should be disposed when it is not needed anymore - to release all Resolve()d objects
        /// </summary>
        IDiResolveReleaseOnlyContainer CreateScope();
    }

    public static class DiResolveReleaseOnlyContainerExtensions {
        public static T Resolve<T>(this IDiResolveReleaseOnlyContainer c) => (T) c.Resolve(typeof(T));
        public static (bool,T) TryResolve<T>(this IDiResolveReleaseOnlyContainer c) {
            var (success, result) = c.TryResolve(typeof(T));
            return (success, (T)result);
        }

        public static IEnumerable<T> ResolveAll<T>(this IDiResolveReleaseOnlyContainer c) => c.ResolveAll(typeof(T)).Cast<T>();
    }
}
