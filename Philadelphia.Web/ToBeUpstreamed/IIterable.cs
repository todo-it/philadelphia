using Bridge;

namespace Philadelphia.Web {
    [External]
    public class IIterable<T> {
        public virtual extern ValueAndDone<T> next();
    }
}
